using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Analysis;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.JsResolvers;
using DotNetWebToolkit.Cil2Js.Utils;
using System.Reflection;
using System.Diagnostics;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class Js {

        class ExprVarCluster : ExprVar {

            public ExprVarCluster(IEnumerable<ExprVar> vars)
                : base(null) {
                this.Vars = vars;
            }

            public IEnumerable<ExprVar> Vars { get; private set; }

            public override Expr.NodeType ExprType {
                get { throw new NotImplementedException(); }
            }

            public override TypeReference Type {
                get { throw new NotImplementedException(); }
            }

        }

        public static string CreateFrom(MethodReference method, bool verbose = false, bool testing = false) {
            return CreateFrom(new[] { method }, verbose, testing);
        }

        public static string CreateFrom(IEnumerable<MethodReference> rootMethods, bool verbose = false, bool testing = false) {
            var todo = new Stack<MethodReference>();
            foreach (var method in rootMethods) {
                todo.Push(method);
            }
            // Each method, with the count of how often it is referenced.
            var methodsSeen = new Dictionary<MethodReference, int>(rootMethods.ToDictionary(x => x, x => 1), TypeExtensions.MethodRefEqComparerInstance);
            // Each type, with the count of how often it is referenced directly (newobj only at the moment).
            var typesSeen = new Dictionary<TypeReference, int>(TypeExtensions.TypeRefEqComparerInstance);
            // ASTs of all methods
            var methodAsts = new Dictionary<MethodReference, ICode>(TypeExtensions.MethodRefEqComparerInstance);
            // Each field, with the count of how often it is referenced.
            var fieldAccesses = new Dictionary<FieldReference, int>(TypeExtensions.FieldReqEqComparerInstance);
            // Each type records which virtual methods have their NewSlot definitions
            var virtualCalls = new Dictionary<TypeReference, HashSet<MethodReference>>(TypeExtensions.TypeRefEqComparerInstance);
            // Each basemost virtual method records the least-derived type actually called
            // This allows only virtual methods in more-derived types to be transcoded
            var virtualCallExactMethods = new Dictionary<MethodReference, IEnumerable<TypeReference>>(TypeExtensions.MethodRefEqComparerInstance);
            // Each interface type records which interface methods are called
            var interfaceCalls = new Dictionary<TypeReference, HashSet<MethodReference>>(TypeExtensions.TypeRefEqComparerInstance);
            // All instance constructors must be updated after all methods have been processed, to initialise all referenced
            // instance fields in the type. This has to be done later, so the list of referenced fields in complete
            var instanceConstructors = new List<Ctx>();

            while (todo.Any()) {
                var mRef = todo.Pop();
                var mDef = mRef.Resolve();
                var tRef = mRef.DeclaringType;
                var tDef = tRef.Resolve();
                var ctx = new Ctx(tRef, mRef);
                var ast = (ICode)JsResolver.ResolveMethod(ctx);
                if (ast == null) {
                    if (tRef.HasGenericParameters) {
                        throw new InvalidOperationException("Type must not have generic parameters");
                    }
                    if (mRef.HasGenericParameters) {
                        throw new InvalidOperationException("Method must not have generic parameters");
                    }
                    if (mDef.IsAbstract) {
                        throw new InvalidOperationException("Cannot transcode an abstract method");
                    }
                    if (mDef.IsInternalCall) {
                        throw new InvalidOperationException("Cannot transcode an internal call");
                    }
                    if (mDef.IsExternal()) {
                        throw new InvalidOperationException("Cannot transcode an external method");
                    }
                    if (!mDef.HasBody) {
                        throw new InvalidOperationException("Cannot transcode method without body");
                    }

                    if (!typesSeen.ContainsKey(tRef)) {
                        typesSeen.Add(tRef, 0);
                    }
                    ast = Transcoder.ToAst(ctx, verbose);
                }

                for (int i = 0; ; i++) {
                    var astOrg = ast;
                    ast = VisitorJsResolve64Bit.V(ast);
                    ast = VisitorJsRewriteSealedVCalls.V(ast);
                    ast = VisitorJsResolveAll.V(ast);
                    ast = VisitorJsResolveConv.V(ast);
                    if (ast == astOrg) {
                        break;
                    }
                    if (i > 10) {
                        // After 10 iterations even the most complex method should be sorted out
                        throw new InvalidOperationException("Error: Stuck in loop trying to resolve AST");
                    }
                }

                if (mDef.IsVirtual && mRef.DeclaringType.IsValueType) {
                    // 'this' may be boxed or unboxed. Must be unboxed if boxed
                    // This is required because in real .NET the boxed and unboxed versions are both directly
                    // available at the this reference; this is not the case in the JS emulation of boxing
                    var unboxJs = "if(this._)this=this.v;";
                    var unbox = new StmtJsExplicit(ctx, unboxJs, ctx.This.Named("this"));
                    ast = new StmtBlock(ctx, unbox, (Stmt)ast);
                }

                if (mDef.IsConstructor && !mDef.IsStatic) {
                    // Instance constructor; add instance field initialisation and final return of 'this' later
                    instanceConstructors.Add(ctx);
                }

                var cctors = VisitorFindStaticConstructors.V(ast).Where(x => !TypeExtensions.MethodRefEqComparerInstance.Equals(x, mRef)).ToArray();
                if (cctors.Any()) {
                    // All methods that access static fields or methods must call the static constructor at the very
                    // start of the method. Except the static construtor itself, which must not recurse into itself.
                    var cctorCalls = cctors
                        .Select(x => new StmtWrapExpr(ctx, new ExprCall(ctx, x, null, Enumerable.Empty<Expr>(), false))).ToArray();
                    ast = new StmtBlock(ctx, cctorCalls.Concat((Stmt)ast));
                }

                if (mDef.IsConstructor && mDef.IsStatic) {
                    // At the end of the static constructor, it rewrites itself as an empty function, so it is only called once.
                    var rewrite = new StmtAssignment(ctx, new ExprJsVarMethodReference(ctx, mRef), new ExprJsEmptyFunction(ctx));
                    ast = new StmtBlock(ctx, (Stmt)ast, rewrite);
                }

                methodAsts.Add(mRef, ast);

                var fieldRefs = VisitorFindFieldAccesses.V(ast);
                foreach (var fieldRef in fieldRefs) {
                    fieldAccesses[fieldRef] = fieldAccesses.ValueOrDefault(fieldRef) + 1;
                }
                var arrayRefs = VisitorFindNewArrays.V(ast);
                foreach (var arrayRef in arrayRefs) {
                    typesSeen[arrayRef] = typesSeen.ValueOrDefault(arrayRef) + 1;
                }
                var types = VisitorFindRequiredTypes.V(ast);
                foreach (var type in types) {
                    typesSeen[type] = typesSeen.ValueOrDefault(type) + 1;
                }

                var calledMethods = new List<ICall>();
                var calls = VisitorFindCalls.V(ast);
                foreach (var call in calls.Where(x => x.ExprType == Expr.NodeType.NewObj || x.IsVirtualCall)) {
                    // Add reference to each type constructed (direct access to type variable)
                    typesSeen[call.Type] = typesSeen.ValueOrDefault(call.Type) + 1;
                }
                foreach (var call in calls) {
                    if (call.CallMethod.DeclaringType.Resolve().IsInterface) {
                        var methodSet = interfaceCalls.ValueOrDefault(call.CallMethod.DeclaringType, () => new HashSet<MethodReference>(TypeExtensions.MethodRefEqComparerInstance), true);
                        methodSet.Add(call.CallMethod);
                        // Methods that require transcoding are added to 'todo' later
                        continue;
                    }
                    if (call.IsVirtualCall) {
                        var mBasemost = call.CallMethod.GetBasemostMethod(null);
                        var methodSet = virtualCalls.ValueOrDefault(mBasemost.DeclaringType, () => new HashSet<MethodReference>(TypeExtensions.MethodRefEqComparerInstance), true);
                        methodSet.Add(mBasemost);
                        var objType = call.Obj.Type;
                        var already = virtualCallExactMethods.ValueOrDefault(mBasemost).EmptyIfNull();
                        if (!already.Any(x => x.IsBaseOfOrEqual(objType))) {
                            virtualCallExactMethods[mBasemost] = already.Concat(objType).ToArray();
                        }
                        // Methods that require transcoding are added to 'todo' later
                        continue;
                    }
                    calledMethods.Add(call);
                }
                foreach (var call in calledMethods) {
                    if (methodsSeen.ContainsKey(call.CallMethod)) {
                        methodsSeen[call.CallMethod]++;
                    } else {
                        methodsSeen.Add(call.CallMethod, 1);
                        todo.Push(call.CallMethod);
                    }
                }

                if (!todo.Any()) {
                    // Add System.RuntimeType if any types have been seen
                    if (typesSeen.Any(x => x.Value > 0) && !typesSeen.Any(x => x.Key.FullName == "System.RuntimeType")) {
                        var runtimeType = ctx.Module.Import(Type.GetType("System.RuntimeType"));
                        typesSeen.Add(runtimeType, 1);
                    }
                    // Scan all virtual calls and add any required methods
                    // Need care to handle virtual methods with generic arguments
                    var virtualRoots = new HashSet<MethodReference>(virtualCalls.SelectMany(x => x.Value), TypeExtensions.MethodRefEqComparerInstance);
                    var requireMethods =
                        from type in typesSeen.Keys
                        let typeAndBases = type.EnumThisAllBaseTypes().ToArray()
                        let mVRoots = typeAndBases.SelectMany(x => virtualCalls.ValueOrDefault(x).EmptyIfNull()).ToArray()
                        let methods = type.EnumResolvedMethods(mVRoots).ToArray()
                        from method in methods
                        let methodDef = method.Resolve()
                        where !methodDef.IsStatic && methodDef.IsVirtual && !methodDef.IsAbstract
                        where !methodsSeen.ContainsKey(method)
                        let mBasemost = method.GetBasemostMethod(method)
                        where virtualCallExactMethods.ValueOrDefault(mBasemost).EmptyIfNull().Any(x => x.IsBaseOfOrEqual(type) || type.IsBaseOfOrEqual(x))
                        where virtualRoots.Contains(mBasemost)
                        select method;
                    var requireMethodsArray = requireMethods.Distinct(TypeExtensions.MethodRefEqComparerInstance).ToArray();
                    foreach (var method in requireMethodsArray) {
                        methodsSeen.Add(method, 1); // TODO: How to properly handle count?
                        todo.Push(method);
                    }
                    // Scan all interface calls and add any required methods
                    var iFaceMethods =
                        from type in typesSeen.Keys
                        from iFace in interfaceCalls
                        let iFaceType = iFace.Key
                        let typeAndBases = type.EnumThisAllBaseTypes().ToArray()
                        where typeAndBases.Any(x => x.DoesImplement(iFaceType))
                        let methods = typeAndBases.SelectMany(x => x.EnumResolvedMethods(iFace.Value)).ToArray()
                        from method in methods
                        where !methodsSeen.ContainsKey(method)
                        let methodDef = method.Resolve()
                        where !methodDef.IsStatic && methodDef.IsVirtual && !methodDef.IsAbstract
                        from iFaceMethod in iFace.Value
                        where method.IsImplementationOf(iFaceMethod)
                        select method;
                    var iFaceMethodsArray = iFaceMethods.Distinct(TypeExtensions.MethodRefEqComparerInstance).ToArray();
                    foreach (var method in iFaceMethodsArray) {
                        methodsSeen.Add(method, 1);
                        todo.Push(method);
                    }
                }
            }

            // Add type count to System.RuntimeType
            var runtimeTypeInc = typesSeen.Keys.FirstOrDefault(x => x.FullName == "System.RuntimeType");
            if (runtimeTypeInc != null) {
                typesSeen[runtimeTypeInc] += 2;
            }

            // Make sure fields of nullable types are named
            var nullables = typesSeen.Keys.Where(x => x.IsNullable()).ToArray();
            foreach (var nullable in nullables) {
                foreach (var field in nullable.EnumResolvedFields()) {
                    if (!fieldAccesses.ContainsKey(field)) {
                        fieldAccesses.Add(field, 1);
                    }
                }
            }

            //// Make sure fields of _Int64 and _UInt64 are names
            //var special64Bits = typesSeen.Keys.Where(x => {
            //    var revMapped = JsResolver.ReverseTypeMap(x);
            //    return revMapped.IsInt64() || revMapped.IsUInt64();
            //}).ToArray();
            //foreach (var bit64 in special64Bits) {
            //    var hi = bit64.GetField("hi");
            //    var lo = bit64.GetField("lo");
            //    // TODO: Counts are not correct (and this whole thing can probably be done better)
            //    if (!fieldAccesses.ContainsKey(hi)) {
            //        fieldAccesses.Add(hi, 1);
            //    }
            //    if (!fieldAccesses.ContainsKey(lo)) {
            //        fieldAccesses.Add(lo, 1);
            //    }
            //}

            var instanceFieldsByType = fieldAccesses
                .Where(x => !x.Key.Resolve().IsStatic)
                .ToLookup(x => x.Key.DeclaringType.FullResolve(x.Key), TypeExtensions.TypeRefEqComparerInstance);
            // Update all instance constructors to initialise instance fields, and add final 'return' statement
            foreach (var ctx in instanceConstructors) {
                var fields = instanceFieldsByType[ctx.TRef].Select(x => x.Key);
                var initStmts = fields.Select(x => {
                    var f = x.FullResolve(ctx.TRef, ctx.MRef);
                    var assign = new StmtAssignment(ctx,
                        new ExprFieldAccess(ctx, ctx.This, f),
                        new ExprDefaultValue(ctx, f.FieldType));
                    return assign;
                })
                    .ToArray();
                var returnStmt = new StmtReturn(ctx, ctx.This);
                var ast = methodAsts[ctx.MRef];
                ast = new StmtBlock(ctx, initStmts.Concat((Stmt)ast).Concat(returnStmt));
                methodAsts[ctx.MRef] = ast;
            }

            // Locally name all instance fields; base type names must not be re-used in derived types
            var instanceFieldsIndexed = new Dictionary<int, Tuple<IEnumerable<FieldReference>, int>>(); // <index, Tuple<all fields, total use count>>
            instanceFieldsByType.TypeTreeTraverse(x => x.Key, (fields, idx) => {
                var ordered = fields.OrderByDescending(x => x.Value).ToArray(); // Order by usage count, highest first
                foreach (var field in ordered) {
                    var idxInfo = instanceFieldsIndexed.ValueOrDefault(idx, () => Tuple.Create(Enumerable.Empty<FieldReference>(), 0));
                    var newIdxInfo = Tuple.Create((IEnumerable<FieldReference>)idxInfo.Item1.Concat(field.Key).ToArray(), idxInfo.Item2 + field.Value);
                    instanceFieldsIndexed[idx] = newIdxInfo;
                    idx++;
                }
                return idx;
            }, 0);
            var orderedInstanceFields = instanceFieldsIndexed.OrderByDescending(x => x.Value.Item2).ToArray();
            var instanceFieldNameGen = new NameGenerator();
            var instanceFieldNames = orderedInstanceFields
                .Select(x => new { name = instanceFieldNameGen.GetNewName(), fields = x.Value.Item1 })
                .SelectMany(x => x.fields.Select(y => new { f = y, name = x.name }))
                .ToArray();
            // Prepare list of static fields for global naming
            var staticFields = fieldAccesses.Where(x => x.Key.Resolve().IsStatic).ToArray();

            // Prepare local variables for global naming.
            // All locals in all methods are sorted by usage count, then all methods usage counts are combined
            var clusters = methodAsts.Values.SelectMany(x => VisitorPhiClusters.V(x).Select(y => new ExprVarCluster(y))).ToArray();
            var varToCluster = clusters.SelectMany(x => x.Vars.Select(y => new { cluster = x, var = y })).ToDictionary(x => x.var, x => x.cluster);
            var varsWithCount = methodAsts.Values.Select(x => {
                var methodVars = VisitorGetVars.V(x);
                // Parameters need one extra count, as they appear in the method declaration
                methodVars = methodVars.Concat(methodVars.Where(y => y.ExprType == Expr.NodeType.VarParameter).Distinct());
                var ret = methodVars.Select(y => varToCluster.ValueOrDefault(y) ?? y)
                    .GroupBy(y => y)
                    .Select(y => new { var = y.Key, count = y.Count() })
                    .OrderByDescending(y => y.count)
                    .ToArray();
                return ret;
            }).ToArray();
            var localVarCounts = new Dictionary<int, int>();
            foreach (var x in varsWithCount) {
                for (int i = 0; i < x.Length; i++) {
                    localVarCounts[i] = localVarCounts.ValueOrDefault(i) + x[i].count;
                }
            }

            // Globally name all items that require names
            var needNaming =
                localVarCounts.Select(x => new { item = (object)x.Key, count = x.Value })
                .Concat(methodsSeen.Select(x => new { item = (object)x.Key, count = x.Value }))
                .Concat(staticFields.Select(x => new { item = (object)x.Key, count = x.Value }))
                .Concat(typesSeen.Select(x => new { item = (object)x.Key, count = x.Value }))
                .Where(x => x.count > 0)
                .OrderByDescending(x => x.count)
                .ToArray();
            var nameGen = new NameGenerator();
            var globalNames = needNaming.ToDictionary(x => x.item, x => nameGen.GetNewName());

            // Create map of all local variables to their names
            var localVarNames = varsWithCount.Select(x => x.Select((y, i) => new { y.var, name = globalNames[i] }))
                .SelectMany(x => x)
                .SelectMany(x => {
                    var varCluster = x.var as ExprVarCluster;
                    if (varCluster != null) {
                        return varCluster.Vars.Select(y => new { var = y, name = x.name });
                    } else {
                        return new[] { x };
                    }
                })
                .ToDictionary(x => x.var, x => x.name);

            // Create map of all method names
            var methodNames = methodsSeen.Keys.ToDictionary(x => x, x => globalNames[x], TypeExtensions.MethodRefEqComparerInstance);
            //methodNames[rootMethods.First()] = "main"; // HACK

            // Create list of all static field names
            var staticFieldNames = staticFields.Select(x => new { f = x.Key, name = globalNames[x.Key] });
            // Create map of all fields
            var fieldNames = instanceFieldNames.Concat(staticFieldNames).ToDictionary(x => x.f, x => x.name, TypeExtensions.FieldReqEqComparerInstance);
            // Create map of type names
            var typeNames = typesSeen
                .Where(x => x.Value > 0)
                .ToDictionary(x => x.Key, x => globalNames[x.Key], TypeExtensions.TypeRefEqComparerInstance);

            // Create virtual call tables
            var virtualCallIndices = new Dictionary<MethodReference, int>(TypeExtensions.MethodRefEqComparerInstance);
            var allVirtualMethods = new Dictionary<TypeReference, HashSet<MethodReference>>(TypeExtensions.TypeRefEqComparerInstance);
            typesSeen.Select(x => x.Key).TypeTreeTraverse(x => x, (type, vCalls) => {
                var mNewSlots = virtualCalls.ValueOrDefault(type).EmptyIfNull().ToArray();
                int idx = vCalls.Length;
                foreach (var mNewSlot in mNewSlots) {
                    virtualCallIndices[mNewSlot] = idx++;
                }
                var vCallsWithThisType = vCalls.Concat(mNewSlots).ToArray();
                if (vCallsWithThisType.Length > 0) {
                    var typesAndBases = type.EnumThisAllBaseTypes().ToArray();
                    var mVRoots = typesAndBases.SelectMany(x => virtualCalls.ValueOrDefault(x).EmptyIfNull()).ToArray();
                    var ms = type.EnumResolvedMethods(mVRoots).ToArray();
                    for (int i = 0; i < vCalls.Length; i++) {
                        var mVCall = vCallsWithThisType[i];
                        foreach (var m in ms) {
                            if (m.MatchMethodOnly(mVCall)) {
                                vCallsWithThisType[i] = m;
                            }
                        }
                    }
                    var typeVMethods = new HashSet<MethodReference>(vCallsWithThisType, TypeExtensions.MethodRefEqComparerInstance);
                    allVirtualMethods.Add(type, typeVMethods);
                }
                return vCallsWithThisType;
            }, new MethodReference[0]);

            var typeData = Enum.GetValues(typeof(TypeData)).Cast<TypeData>().ToArray();

            // Name all items that are within the type information
            var needTypeInformationNaming =
                interfaceCalls.Select(x => new { item = (object)x.Key, count = 1 })
                .Concat(typeData.Select(x => new { item = (object)x, count = 1 }))
                .OrderByDescending(x => x.count)
                .ToArray();
            var typeInformationNameGen = new NameGenerator();
            var typeInformationNames = needTypeInformationNaming.ToDictionary(x => x.item, x => typeInformationNameGen.GetNewName());

            // Create map of interfaces to their names
            var interfaceNames = interfaceCalls.Keys.ToDictionary(x => x, x => typeInformationNames[x], TypeExtensions.TypeRefEqComparerInstance);
            var interfaceCallIndices = interfaceCalls.SelectMany(x => x.Value.Select((m, i) => new { m, i })).ToDictionary(x => x.m, x => x.i, TypeExtensions.MethodRefEqComparerInstance);

            // Create map of type data constants
            var typeDataNames = typeData.ToDictionary(x => x, x => typeInformationNames[x]);

            var resolver = new JsMethod.Resolver {
                LocalVarNames = localVarNames,
                MethodNames = methodNames,
                FieldNames = fieldNames,
                TypeNames = typeNames,
                VirtualCallIndices = virtualCallIndices,
                InterfaceCallIndices = interfaceCallIndices,
                InterfaceNames = interfaceNames,
                TypeDataNames = typeDataNames,
            };

            var js = new StringBuilder();
            js.Append("(function(){");
            int jsIndent = 1;
            Action jsNewLine = () => {
                js.AppendLine();
                js.Append(' ', jsIndent * JsMethod.tabSize);
            };

            jsNewLine();
            // Construct methods
            foreach (var methodInfo in methodAsts) {
                var mRef = methodInfo.Key;
                var ast = methodInfo.Value;
                var mJs = JsMethod.Create(mRef, resolver, ast);
                var mJsLines = mJs.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (var line in mJsLines) {
                    jsNewLine();
                    js.Append(line);
                }
            }

            // Construct static fields
            foreach (var field in staticFields.Select(x => x.Key)) {
                jsNewLine();
                js.AppendFormat("// {0}", field.FullName);
                jsNewLine();
                if (field.Name == "Empty" && field.DeclaringType.FullName == "System.String") {
                    // Special case, as string does not have a static constructor to set String.Empty
                    js.AppendFormat("var {0} = \"\";", fieldNames[field]);
                } else {
                    js.AppendFormat("var {0} = {1};", fieldNames[field], DefaultValuer.Get(field.FieldType, fieldNames));
                }
            }

            // Construct type data
            var typesSeenOrdered = typesSeen
                .Where(x => x.Value > 0)
                .Select(x => x.Key)
                .OrderByReferencedFirst(x => x)
                .ToArray();
            foreach (var type in typesSeenOrdered) {
                var unmappedType = JsResolver.ReverseTypeMap(type);
                var tDef = unmappedType.Resolve();
                jsNewLine();
                js.AppendFormat("// {0}", unmappedType.FullName);
                jsNewLine();
                js.AppendFormat("var {0}={{", typeNames[type]);
                // Type information
                js.AppendFormat("{0}:\"{1}\"", typeDataNames[TypeData.Name], unmappedType.Name());
                js.AppendFormat(", {0}:\"{1}\"", typeDataNames[TypeData.Namespace], unmappedType.Namespace);
                js.AppendFormat(", {0}:{1}", typeDataNames[TypeData.IsValueType], unmappedType.IsValueType ? "true" : "false");
                js.AppendFormat(", {0}:{1}", typeDataNames[TypeData.IsArray], unmappedType.IsArray ? "true" : "false");
                js.AppendFormat(", {0}:{1}", typeDataNames[TypeData.ElementType], unmappedType.IsArray ? typeNames.ValueOrDefault(unmappedType.GetElementType(), "null") : "null");
                js.AppendFormat(", {0}:{1}", typeDataNames[TypeData.IsInterface], tDef.IsInterface ? "true" : "false");
                var assignableTo = typesSeenOrdered.Where(x => unmappedType.IsAssignableTo(x)).Where(x => !x.IsSame(unmappedType)).ToArray();
                js.AppendFormat(", {0}:[{1}]", typeDataNames[TypeData.AssignableTo], string.Join(", ", assignableTo.Select(x => typeNames[x])));
                if (!tDef.IsInterface) {
                    if (!tDef.IsAbstract) {
                        // Virtual method table, only needed on concrete types
                        var typeAndBases = type.EnumThisAllBaseTypes().ToArray();
                        var methods = allVirtualMethods.ValueOrDefault(type);
                        if (methods != null) {
                            var idxs = methods
                                .Select(x => {
                                    var xBasemost = x.GetBasemostMethod(x);
                                    return new { m = x, idx = virtualCallIndices[xBasemost] };
                                })
                                .OrderBy(x => x.idx)
                                .ToArray();
                            var s = string.Join(", ", idxs.Select(x => methodNames.ValueOrDefault(x.m, "null")));
                            js.AppendFormat(", {0}:[{1}]", typeDataNames[TypeData.VTable], s);
                        }
                        // Interface tables, only needed on concrete types
                        var implementedIFaces = interfaceCalls.Where(x => typeAndBases.Any(y => y.DoesImplement(x.Key))).ToArray();
                        foreach (var iFace in implementedIFaces) {
                            js.Append(", ");
                            var iFaceName = interfaceNames[iFace.Key];
                            js.AppendFormat("{0}:[", iFaceName);
                            var qInterfaceTableNames =
                                from iMethod in iFace.Value
                                let tMethod = typeAndBases.SelectMany(x => x.EnumResolvedMethods(iMethod)).First(x => x.IsImplementationOf(iMethod))
                                let idx = interfaceCallIndices[iMethod]
                                orderby idx
                                let methodName = methodNames[tMethod]
                                select methodName;
                            var interfaceTableNames = qInterfaceTableNames.ToArray();
                            js.Append(string.Join(", ", interfaceTableNames));
                            js.Append("]");
                        }
                    }
                }
                // end
                js.Append("};");
            }
            // Add type of each type, if System.RuntimeType has been seen
            var typeRuntimeType = typesSeen.Keys.FirstOrDefault(x => x.FullName == "System.RuntimeType");
            if (typeRuntimeType != null) {
                jsNewLine();
                foreach (var type in typesSeenOrdered) {
                    js.Append(typeNames[type]);
                    js.Append("._ = ");
                }
                js.Append(typeNames[typeRuntimeType]);
                js.Append(";");
            }

            jsNewLine();
            jsNewLine();
            js.Append("// Exports");
            if (!testing) {
                var rootMethodsByType = rootMethods.ToLookup(x => x.DeclaringType, TypeExtensions.TypeRefEqComparerInstance);
                Action<NamespaceTree> treeToJs = null;
                treeToJs = tree => {
                    js.Append("{");
                    jsIndent++;
                    foreach (var subNs in tree.Namespaces) {
                        jsNewLine();
                        js.AppendFormat("'{0}': ", subNs.NamespacePart);
                        treeToJs(subNs);
                    }
                    foreach (var type in tree.Types) {
                        jsNewLine();
                        js.AppendFormat("'{0}': {{", type.Name);
                        jsIndent++;
                        foreach (var method in rootMethodsByType[type]) {
                            jsNewLine();
                            js.AppendFormat("'{0}': {1}", method.Name, methodNames[method]);
                        }
                        jsIndent--;
                        jsNewLine();
                        js.Append("}");
                    }
                    jsIndent--;
                    jsNewLine();
                    js.Append("}");
                };
                var trees = NamespaceTree.Make(rootMethodsByType.Select(x => x.Key));
                foreach (var tree in trees) {
                    jsNewLine();
                    js.AppendFormat("window['{0}'] = ", tree.NamespacePart);
                    treeToJs(tree);
                    js.Append(";");
                }
            } else {
                jsNewLine();
                js.AppendFormat("window['main'] = {0};", methodNames[rootMethods.First()]);
            }

            jsIndent--;
            jsNewLine();
            jsNewLine();
            js.Append("})();");

            var jsStr = js.ToString();
            //Console.WriteLine(jsStr);
            return jsStr;
        }

    }
}
