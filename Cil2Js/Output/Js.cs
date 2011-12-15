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

namespace Cil2Js.Output {
    public class Js {

        public static string CreateFrom(MethodInfo methodInfo, bool verbose = false) {
            return CreateFrom(Transcoder.GetMethod(methodInfo), verbose);
        }

        public static string CreateFrom(MethodDefinition method, bool verbose = false) {
            return CreateFrom(new[] { method }, verbose);
        }

        public static string CreateFrom(IEnumerable<MethodDefinition> rootMethods, bool verbose = false) {
            var todo = new Queue<MethodDefinition>();
            foreach (var method in rootMethods) {
                todo.Enqueue(method);
            }
            var seen = new HashSet<MethodDefinition>(rootMethods);
            var typesSeen = new HashSet<TypeDefinition>();
            var asts = new Dictionary<MethodDefinition, ICode>();
            var callResolvers = new Dictionary<ICall, JsResolved>();
            var exports = new List<Tuple<MethodDefinition, string>>();
            var fields = new Dictionary<FieldDefinition, int>();
            // Key is the base-most method (possibly abstract), hashset contains base and all overrides that are not abstract
            var virtualCalls = new Dictionary<MethodDefinition, HashSet<MethodDefinition>>();
            var cctorCalls = new Dictionary<MethodDefinition, IEnumerable<MethodDefinition>>();
            // Which interface methods are referenced; key is interface, hashset contains all referenced methods on key interface
            var interfaceCalls = new Dictionary<TypeDefinition, HashSet<MethodDefinition>>();

            while (todo.Any()) {
                var method = todo.Dequeue();
                if (method.IsAbstract) {
                    continue;
                }
                if (method.IsInternalCall) {
                    throw new ArgumentException("Cannot transcode an internal method");
                }

                typesSeen.Add(method.DeclaringType);
                if (method.IsVirtual && !method.IsAbstract) {
                    var baseMethod = method.GetBasemostMethodInTypeHierarchy();
                    virtualCalls.ValueOrDefault(baseMethod, () => new HashSet<MethodDefinition>(), true).Add(method);
                }
                // Is it exported?
                var export = method.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "Cil2Js.Attributes.ExportAttribute");
                if (export != null) {
                    var exportName = (string)export.ConstructorArguments.FirstOrDefault().Value ?? method.Name;
                    exports.Add(Tuple.Create(method, exportName));
                }
                // Create AST
                var exprGen = Expr.ExprGen(method.Module.TypeSystem);
                var ast = Transcoder.ToAst(method, verbose);
                var methodsCalled = new List<MethodDefinition>();
                for (int i = 0; ; i++) {
                    var astOrg = ast;
                    // Resolve calls
                    var vResolveCalls = new VisitorResolveCalls(call => JsCallResolver.Resolve(exprGen, call));
                    ast = vResolveCalls.Visit(ast);
                    // Resolve field accesses
                    var vResolveFieldAccess = new VisitorResolveFieldAccess(field => null);
                    ast = vResolveFieldAccess.Visit(ast);
                    // Check if more rewrites may be required
                    if (astOrg == ast) {
                        break;
                    }
                    if (i > 10) {
                        // After 10 iterations even the most complex method should be sorted out
                        throw new InvalidOperationException("Error: Stuck in loop trying to resolve AST");
                    }
                }
                // Examine all called methods
                var calls = VisitorFindCalls.V(ast);
                foreach (var callInfo in calls) {
                    var call = callInfo.Item1;
                    if (call.CallMethod.DeclaringType.IsInterface) {
                        interfaceCalls.ValueOrDefault(call.CallMethod.DeclaringType, () => new HashSet<MethodDefinition>(), true)
                            .Add(call.CallMethod);
                    } else {
                        var isVirtual = callInfo.Item2;
                        var resolved = JsCallResolver.Resolve(exprGen, call);
                        if (resolved == null) {
                            var callMethod = call.CallMethod;
                            if (isVirtual && callMethod.IsVirtual) {
                                var baseMethod = callMethod.GetBasemostMethodInTypeHierarchy();
                                virtualCalls.ValueOrDefault(baseMethod, () => new HashSet<MethodDefinition>(), true);
                            }
                            methodsCalled.Add(call.CallMethod);
                        } else {
                            switch (resolved.Type) {
                            case JsResolvedType.Method:
                            case JsResolvedType.Property:
                                callResolvers.Add(call, resolved);
                                break;
                            default:
                                throw new NotImplementedException("Cannot handle: " + resolved.Type);
                            }
                        }
                    }
                }
                // Record all field accesses
                var fieldAccesses = VisitorFindFieldAccesses.V(ast);
                foreach (var fieldAccess in fieldAccesses) {
                    fields[fieldAccess] = fields.ValueOrDefault(fieldAccess, () => 0, true) + 1;
                }
                // Find all static constructors that need calling/converting
                var staticConstructors = VisitorFindStaticConstructors.V(ast);
                methodsCalled.AddRange(staticConstructors);
                cctorCalls.Add(method, staticConstructors);
                // Record this converted AST
                asts.Add(method, ast);

                // Queue any methods that now need converting to JS
                Action<IEnumerable<MethodDefinition>> addToTodo = toAdd => {
                    foreach (var call in toAdd) {
                        if (seen.Add(call)) {
                            todo.Enqueue(call);
                        }
                    }
                };

                addToTodo(methodsCalled);

                // When run out of methods, add any extra methods needed from virtual calls and interface calls
                if (!todo.Any()) {
                    // Virtual calls
                    var vToAdd =
                        from type in typesSeen
                        from m in type.Methods
                        where !m.IsAbstract && m.IsVirtual
                        let mBase = m.GetBasemostMethodInTypeHierarchy()
                        where virtualCalls.ContainsKey(mBase)
                        select m;
                    var vToAddArray = vToAdd.ToArray();
                    addToTodo(vToAddArray);
                    // Interface calls
                    var iToAdd =
                        from type in typesSeen
                        from iFaceRef in type.Interfaces
                        let iFace = iFaceRef.Resolve()
                        let iFaceMethods = interfaceCalls.ValueOrDefault(iFace)
                        where iFaceMethods != null
                        from iFaceMethod in iFaceMethods
                        let m = type.GetInterfaceMethod(iFaceMethod)
                        where !m.IsAbstract
                        select m;
                    var iToAddArray = iToAdd.ToArray();
                    addToTodo(iToAddArray);
                }
            }

            // Name all methods
            // TODO: Improve this
            var methodNamer = new NameGenerator("$");
            var methodNames = asts.Keys.ToDictionary(x => x, x => methodNamer.GetNewName());
            methodNames[rootMethods.First()] = "main";
            // Name all fields
            // TODO: This generates bad quality names as no names are shared over different types, to improve...
            var fieldNamer = new NameGenerator();
            var staticFieldNamer = new NameGenerator("$_");
            var fieldNames = fields.Keys.ToDictionary(x => x, x => {
                if (x.IsStatic) {
                    return staticFieldNamer.GetNewName();
                } else {
                    return fieldNamer.GetNewName();
                }
            });
            // Create vTables for virtual methods
            var vMethodsByType = virtualCalls.SelectMany(x => x.Value.Concat(x.Key).Distinct()).ToLookup(x => x.DeclaringType);
            var allTypesInBaseOrder = typesSeen.OrderByBaseFirst().ToArray();
            var vTable = new Dictionary<TypeDefinition, MethodDefinition[]>();
            var virtualCalls2 = new Dictionary<MethodDefinition, int>();
            foreach (var vType in allTypesInBaseOrder) {
                MethodDefinition[] vTableBase = null;
                var vTypeBase = vType;
                for (; ; ) {
                    vTypeBase = vTypeBase.GetBaseType();
                    if (vTypeBase == null) {
                        break;
                    }
                    vTableBase = vTable.ValueOrDefault(vTypeBase);
                    if (vTableBase != null) {
                        break;
                    }
                }
                if (vTableBase == null) {
                    vTableBase = new MethodDefinition[0];
                }
                var vTypeVTable = new List<MethodDefinition>(vTableBase);
                if (vMethodsByType.Contains(vType)) {
                    foreach (var method in vMethodsByType[vType]) {
                        // Must always be either a new slot or in base type vTable
                        int idx;
                        if (method.IsNewSlot) {
                            idx = vTypeVTable.Count;
                            vTypeVTable.Add(method);
                        } else {
                            idx = Enumerable.Range(0, vTableBase.Length).First(i => vTableBase[i].MethodMatch(method));
                            vTypeVTable[idx] = method;
                        }
                        virtualCalls2.Add(method, idx);
                    }
                }
                if (vTypeVTable.Any()) {
                    vTable.Add(vType, vTypeVTable.ToArray());
                }
            }
            var vTableNamer = new NameGenerator("_");
            var vTableNames = vTable.Keys.ToDictionary(x => x, x => vTableNamer.GetNewName());
            // Create interface tables
            var interfaceNamer = new NameGenerator();
            var interfaceNames = interfaceCalls.Keys.ToDictionary(x => x, x => interfaceNamer.GetNewName());
            var interfaceMethods = interfaceCalls.Select(x => x.Value.Select((m, i) => new { m, i }))
                .SelectMany(x => x).ToDictionary(x => x.m, x => x.i);
            // Key is each class, value: key=interface type; value=list of methods that implement each interface method
            var interfaceCallsNames = new Dictionary<TypeDefinition, Dictionary<TypeDefinition, Tuple<string, IEnumerable<MethodDefinition>>>>();
            foreach (var type in allTypesInBaseOrder) {
                var iTypes = type.Interfaces.Select(x => x.Resolve());
                foreach (var iType in iTypes) {
                    var iMethods = interfaceCalls.ValueOrDefault(iType).ToArray();
                    if (iMethods != null) {
                        var map = new MethodDefinition[iMethods.Length];
                        foreach (var iMethod in iMethods) {
                            var implMethod = type.GetInterfaceMethod(iMethod);
                            map[interfaceMethods[iMethod]] = implMethod;
                        }
                        interfaceCallsNames.ValueOrDefault(type, () => new Dictionary<TypeDefinition, Tuple<string, IEnumerable<MethodDefinition>>>(), true)
                            .Add(iType, Tuple.Create(vTableNamer.GetNewName(), (IEnumerable<MethodDefinition>)map));
                    }
                }
            }
            var icn = interfaceCallsNames.ToDictionary(x => x.Key, x => x.Value.ToDictionary(y => y.Key, y => y.Value.Item1));

            var js = new StringBuilder();
            // Declare static fields
            var staticFieldInits = fieldNames.Where(x => x.Key.IsStatic)
                .Select(x => x.Value + " = " + DefaultValuer.Get(x.Key.FieldType)).ToArray();
            if (staticFieldInits.Any()) {
                js.AppendFormat("var {0};", string.Join(", ", staticFieldInits));
                js.AppendLine();
                js.AppendLine();
            }

            // Create JS for all methods
            var resolver = new JsMethod.Resolver(methodNames, fieldNames, vTableNames, virtualCalls2, callResolvers,
                cctorCalls, interfaceNames, interfaceMethods, icn);
            foreach (var kv in asts) {
                var method = kv.Key;
                var ast = kv.Value;
                var s = JsMethod.Create(method, resolver, ast);
                js.Append(s);
                js.AppendLine();
            }

            // vTables
            foreach (var vT in vTable) {
                var contents = string.Join(", ", vT.Value.Select(x => methodNames.ValueOrDefault(x, () => "0")));
                js.AppendFormat("var {0} = [{1}];", vTableNames[vT.Key], contents);
                js.AppendLine();
            }

            // Interface calls
            foreach (var iT in interfaceCallsNames) {
                foreach (var i2 in iT.Value) {
                    var contents = string.Join(", ", i2.Value.Item2.Select(x => methodNames[x]));
                    js.AppendFormat("var {0} = [{1}];", i2.Value.Item1, contents);
                    js.AppendLine();
                }
            }

            // Exports
            foreach (var export in exports) {
                js.AppendFormat("window['{0}'] = {1}", export.Item2, methodNames[export.Item1]);
                js.AppendLine();
            }

            var jsStr = js.ToString();
            return jsStr;
        }

    }
}
