using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Cil2Js.Analysis;
using Cil2Js.Ast;
using Cil2Js.JsResolvers;
using Cil2Js.Utils;
using System.Reflection;
using System.Diagnostics;

namespace Cil2Js.Output {
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

        public static string CreateFrom(MethodReference method, bool verbose = false) {
            return CreateFrom(new[] { method }, verbose);
        }

        public static string CreateFrom(IEnumerable<MethodReference> rootMethods, bool verbose = false) {
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
            // All calls that require resolving in special ways, not just by calling the called method
            var resolvedCalls = new Dictionary<ICall, JsResolved>();
            // Each type records which virtual methods have their NewSlot definitions
            var virtualCalls = new Dictionary<TypeReference, HashSet<MethodReference>>(TypeExtensions.TypeRefEqComparerInstance);
            // Each interface type records which interface methods are called
            var interfaceCalls = new Dictionary<TypeReference, HashSet<MethodReference>>(TypeExtensions.TypeRefEqComparerInstance);
            // All instance constructors must be updated after all methods have been processed, to initialise all referenced
            // instance fields in the type. This has to be done later, so the list of referenced fields in complete
            var instanceConstructors = new List<Ctx>();

            var tArray = rootMethods.First().Module.Import(typeof(int[]));
            typesSeen.Add(tArray, 1);

            while (todo.Any()) {
                var mRef = todo.Pop();
                var mDef = mRef.Resolve();
                var tRef = mRef.DeclaringType;
                var tDef = tRef.Resolve();
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

                var ast = Transcoder.ToAst(mRef, tRef, verbose);
                var ctx = ast.Ctx;

                for (int i = 0; ; i++) {
                    var astOrg = ast;
                    var vResolveCalls = new VisitorResolveCalls(JsCallResolver.Resolve);
                    ast = vResolveCalls.Visit(ast);
                    if (ast == astOrg) {
                        break;
                    }
                    if (i > 10) {
                        // After 10 iterations even the most complex method should be sorted out
                        throw new InvalidOperationException("Error: Stuck in loop trying to resolve AST");
                    }
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

                var vCalls = VisitorFindCalls.V(ast).Where(x => x.IsVirtualCall).ToArray();
                foreach (var vCall in vCalls) {
                    // (_ = obj)[vIdx](_, ... args ...)
                    var tempObj = new ExprVarLocal(ctx, vCall.Obj.Type);
                    var jsVCall = new ExprJsVirtualCall(ctx, vCall.CallMethod, new ExprAssignment(ctx, tempObj, vCall.Obj), tempObj, vCall.Args);
                    ast = VisitorReplace.V(ast, vCall, jsVCall);
                }

                methodAsts.Add(mRef, ast);

                var fieldRefs = VisitorFindFieldAccesses.V(ast);
                foreach (var fieldRef in fieldRefs) {
                    fieldAccesses[fieldRef] = fieldAccesses.ValueOrDefault(fieldRef) + 1;
                }

                var calledMethods = new List<ICall>();
                var calls = VisitorFindCalls.V(ast);
                foreach (var call in calls.Where(x => x.ExprType == Expr.NodeType.NewObj)) {
                    // Add reference to each type constructed (direct access to type variable)
                    typesSeen[call.Type] = typesSeen.ValueOrDefault(call.Type) + 1;
                }
                foreach (var call in calls) {
                    var resolved = JsCallResolver.Resolve(call);
                    if (resolved != null) {
                        resolvedCalls.Add(call, resolved);
                        continue;
                    }
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
                    // Scan all virtual calls and add any required methods
                    // Need care to handle virtual methods with generic arguments
                    var virtualRoots = new HashSet<MethodReference>(virtualCalls.SelectMany(x => x.Value), TypeExtensions.MethodRefEqComparerInstance);
                    var requireMethods =
                        from type in typesSeen.Keys
                        let typeAndBases = type.EnumThisAllBaseTypes().ToArray()
                        let mVRoots = typeAndBases.SelectMany(x => virtualCalls.ValueOrDefault(x).EmptyIfNull()).ToArray()
                        let methods = type.EnumResolvedMethods(mVRoots).ToArray()
                        from method in methods
                        where !methodsSeen.ContainsKey(method)
                        let methodDef = method.Resolve()
                        where !methodDef.IsStatic && methodDef.IsVirtual && !methodDef.IsAbstract
                        let mBasemost = method.GetBasemostMethod(method)
                        where virtualRoots.Contains(mBasemost)
                        select method;
                    var requireMethodsArray = requireMethods.ToArray();
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
                    var iFaceMethodsArray = iFaceMethods.ToArray();
                    foreach (var method in iFaceMethodsArray) {
                        methodsSeen.Add(method, 1);
                        todo.Push(method);
                    }
                }
            }

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
            methodNames[rootMethods.First()] = "main"; // HACK

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

            // Create interface call tables
            var interfaceNameGen = new NameGenerator();
            // TODO: Doesn't take use frequency into account yet
            var interfaceNames = interfaceCalls.Keys.ToDictionary(x => x, x => interfaceNameGen.GetNewName());
            var interfaceCallIndices = interfaceCalls.SelectMany(x => x.Value.Select((m, i) => new { m, i })).ToDictionary(x => x.m, x => x.i);

            var resolver = new JsMethod.Resolver {
                LocalVarNames = localVarNames,
                MethodNames = methodNames,
                FieldNames = fieldNames,
                TypeNames = typeNames,
                ResolvedCalls = resolvedCalls,
                VirtualCallIndices = virtualCallIndices,
                InterfaceCallIndices = interfaceCallIndices,
                InterfaceNames = interfaceNames,
            };

            var js = new StringBuilder();

            // Construct methods
            foreach (var methodInfo in methodAsts) {
                var mRef = methodInfo.Key;
                var ast = methodInfo.Value;
                var mJs = JsMethod.Create(mRef, resolver, ast);
                js.AppendLine(mJs);
            }

            // Construct static fields
            foreach (var field in staticFields.Select(x => x.Key)) {
                js.AppendFormat("var {0} = {1};", fieldNames[field], DefaultValuer.Get(field.FieldType));
                js.AppendLine();
            }

            // Construct type data
            foreach (var type in typesSeen.Where(x => x.Value > 0).Select(x => x.Key)) {
                var typeAndBases = type.EnumThisAllBaseTypes().ToArray();
                bool needComma = false;
                js.AppendFormat("var {0}={{", typeNames[type]);
                var methods = allVirtualMethods.ValueOrDefault(type);
                if (methods != null) {
                    //var m
                    var idxs = methods
                        .Select(x => {
                            var xBasemost = x.GetBasemostMethod(x);
                            return new { m = x, idx = virtualCallIndices[xBasemost] };
                        })
                        .OrderBy(x => x.idx)
                        .ToArray();
                    var s = string.Join(", ", idxs.Select(x => methodNames[x.m]));
                    js.AppendFormat("_:[{0}]", s);
                    needComma = true;
                }
                var implementedIFaces = interfaceCalls.Where(x => typeAndBases.Any(y => y.DoesImplement(x.Key))).ToArray();
                foreach (var iFace in implementedIFaces) {
                    if (needComma) {
                        js.Append(", ");
                    } else {
                        needComma = true;
                    }
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
                js.Append("};");
                js.AppendLine();
            }

            var jsStr = js.ToString();
            //Console.WriteLine(jsStr);
            return jsStr;
        }

    }
}
