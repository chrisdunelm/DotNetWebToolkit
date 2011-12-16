using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Analysis;
using Cil2Js.Ast;
using Mono.Cecil;
using Cil2Js.Utils;

namespace Cil2Js.Output {
    public class JsMethod : AstVisitor {

        public class Resolver {

            public Resolver(
                IDictionary<MethodDefinition, string> methodNames,
                IDictionary<FieldDefinition, string> fieldNames,
                IDictionary<TypeDefinition, string> vTables,
                IDictionary<MethodDefinition, int> virtualCalls,
                IDictionary<ICall, JsResolved> callResolvers,
                IDictionary<MethodDefinition, IEnumerable<MethodDefinition>> cctorsCalled,
                IDictionary<TypeDefinition, string> interfaceNames,
                IDictionary<MethodDefinition, int> interfaceMethods,
                IDictionary<TypeDefinition, Dictionary<TypeDefinition, string>> interfaceCallsNames) {
                this.MethodNames = methodNames;
                this.FieldNames = fieldNames;
                this.VTables = vTables;
                this.VirtualCalls = virtualCalls;
                this.CallResolvers = callResolvers;
                this.CctorsCalled = cctorsCalled;
                this.InterfaceNames = interfaceNames;
                this.InterfaceMethods = interfaceMethods;
                this.InterfaceCallsNames = interfaceCallsNames;
            }

            public IDictionary<MethodDefinition, string> MethodNames { get; private set; }
            public IDictionary<FieldDefinition, string> FieldNames { get; private set; }
            public IDictionary<TypeDefinition, string> VTables { get; private set; }
            public IDictionary<MethodDefinition, int> VirtualCalls { get; private set; }
            public IDictionary<ICall, JsResolved> CallResolvers { get; private set; }
            public IDictionary<MethodDefinition, IEnumerable<MethodDefinition>> CctorsCalled { get; private set; }
            public IDictionary<TypeDefinition, string> InterfaceNames { get; private set; }
            public IDictionary<MethodDefinition, int> InterfaceMethods { get; private set; }
            public IDictionary<TypeDefinition, Dictionary<TypeDefinition, string>> InterfaceCallsNames { get; private set; }

        }

        class LocalVarNamer : AstVisitor {

            class VarClustered : ExprVar {

                public VarClustered(TypeSystem typeSystem, IEnumerable<ExprVar> vars) {
                    this.TypeSystem = typeSystem;
                    this.Vars = vars;
                }

                public IEnumerable<ExprVar> Vars { get; private set; }
                public TypeSystem TypeSystem { get; private set; }

                public override Expr.NodeType ExprType {
                    get { throw new NotImplementedException(); }
                }

                public override TypeReference Type {
                    get { return this.Vars.Select(x => x.Type).Aggregate((a, b) => TypeCombiner.Combine(this.TypeSystem, a, b)); }
                }
            }

            public static Dictionary<ExprVar, string> V(TypeSystem typeSystem, ICode c) {
                var v = new LocalVarNamer();
                v.Visit(c);
                var clusters = UniqueClusters(v.clusters);
                var allCounts = clusters.ToDictionary(x => (ExprVar)new VarClustered(typeSystem, x), x => x.Sum(y => v.varCount.ValueOrDefault(y)));
                var allClusteredCounts = allCounts.ToArray();
                var allClustered = new HashSet<ExprVar>(clusters.SelectMany(x => x));
                foreach (var varCount in v.varCount) {
                    if (!allClustered.Contains(varCount.Key)) {
                        allCounts.Add(varCount.Key, varCount.Value);
                    }
                }
                var nameGenerator = new NameGenerator();
                var names = allCounts
                    .OrderByDescending(x => x.Value)
                    .ToDictionary(x => x.Key, x => nameGenerator.GetNewName());
                foreach (var cluster in allClusteredCounts) {
                    foreach (var var in ((VarClustered)cluster.Key).Vars) {
                        names.Add(var, names[cluster.Key]);
                    }
                }
                return names;
            }

            private static IEnumerable<ExprVar[]> UniqueClusters(IEnumerable<ExprVar[]> clusters) {
            start:
                foreach (var a in clusters) {
                    foreach (var b in clusters) {
                        if (a == b) {
                            continue;
                        }
                        if (a.Intersect(b).Any()) {
                            clusters = clusters.Where(x => x != a && x != b).Concat(a.Union(b).ToArray()).ToArray();
                            goto start;
                        }
                    }
                }
                return clusters;
            }

            private LocalVarNamer() { }

            private Dictionary<ExprVar, int> varCount = new Dictionary<ExprVar, int>();
            private List<ExprVar[]> clusters = new List<ExprVar[]>();

            protected override ICode VisitVar(ExprVar e) {
                if (e.ExprType == Expr.NodeType.VarPhi) {
                    var ePhi = (ExprVarPhi)e;
                    this.clusters.Add(ePhi.Exprs.Cast<ExprVar>().Concat(ePhi).ToArray());
                }
                var curCount = this.varCount.ValueOrDefault(e, () => 0);
                this.varCount[e] = curCount + 1;
                return e;
            }

        }

        private const int tabSize = 4;
        private const string thisName = "$";

        public static string Create(MethodDefinition method, Resolver resolver, ICode ast) {
            if (method.IsAbstract) {
                throw new ArgumentException("Cannot transcode an abstract method");
            }
            if (method.IsInternalCall) {
                throw new ArgumentException("Cannot transcode an internal method");
            }
            var methodName = resolver.MethodNames[method];
            var typeSystem = method.Module.TypeSystem;
            var varNames = LocalVarNamer.V(typeSystem, ast);
            var v = new JsMethod(method, varNames, resolver);
            v.Visit(ast);
            var js = v.js.ToString();
            var sb = new StringBuilder();
            var parameterNames = method.Parameters.Select(x => v.parameters.ValueOrDefault(x, () => null).NullThru(y => varNames[y], "_")).ToArray();
            if (!method.IsStatic) {
                parameterNames = new[] { thisName }.Concat(parameterNames).ToArray();
            }
            var fnFmt = method.IsConstructor && method.IsStatic ? "var {0} = function({1})" : "function {0}({1})";
            sb.AppendFormat(fnFmt + " {{", methodName, string.Join(", ", parameterNames));
            var vars = varNames
                .Where(x => !parameterNames.Contains(x.Value))
                .Select(x => x.Value + " = " + DefaultValuer.Get(x.Key.Type))
                .Distinct()
                .Concat(v.needVirtualCallVars)
                .ToArray();
            if (vars.Any()) {
                sb.AppendLine();
                sb.Append(' ', tabSize);
                sb.AppendFormat("var {0};", string.Join(", ", vars.Select(x => x)));
            }

            // Make sure that static constructors are called before any references to static members
            if (method.IsConstructor && method.IsStatic) {
                // If this is a cctor, then mark it as called
                sb.AppendLine();
                sb.Append(' ', tabSize);
                sb.AppendFormat("{0} = null;", methodName);
            }
            foreach (var cctor in resolver.CctorsCalled[method]) {
                if (cctor != method) {
                    // Don't re-call same cctor!
                    sb.AppendLine();
                    sb.Append(' ', tabSize);
                    sb.AppendFormat("if ({0}) {0}();", resolver.MethodNames[cctor]);
                }
            }

            // In a non-static constructor, the object with type information must be constructed
            if (method.IsConstructor && !method.IsStatic && !method.DeclaringType.IsAbstract) {
                sb.AppendLine();
                sb.Append(' ', tabSize);
                sb.AppendFormat("if (!{0}) {0} = {{_:{{", thisName);
                bool needComma = false;
                var vTableName = resolver.VTables.ValueOrDefault(method.DeclaringType);
                if (vTableName != null) {
                    sb.AppendFormat("_:{0}", vTableName);
                    needComma = true;
                }
                var ifaces = method.DeclaringType.GetAllInterfaces().Where(x => resolver.InterfaceNames.ContainsKey(x)).ToArray();
                foreach (var iface in ifaces) {
                    if (needComma) {
                        sb.Append(", ");
                    } else {
                        needComma = true;
                    }
                    sb.AppendFormat("{0}:{1}", resolver.InterfaceNames[iface], resolver.InterfaceCallsNames[method.DeclaringType][iface]);
                }
                sb.Append("}};");
            }

            sb.AppendLine(js);
            sb.AppendLine("}");
            return sb.ToString();
        }

        private JsMethod(MethodDefinition method, Dictionary<ExprVar, string> varNames, Resolver resolver)
            : base(true) {
            this.method = method;
            this.varNames = varNames;
            this.resolver = resolver;
        }

        private MethodDefinition method;
        private Dictionary<ExprVar, string> varNames;
        private Resolver resolver;

        private Dictionary<ParameterDefinition, ExprVar> parameters = new Dictionary<ParameterDefinition, ExprVar>();
        private StringBuilder js = new StringBuilder();
        private List<string> needVirtualCallVars = new List<string>();

        private NameGenerator virtualAndInterfaceCallVars = new NameGenerator("__");
        private int indent = 1;

        private void NewLine() {
            this.js.Append(Environment.NewLine);
            this.js.Append(' ', this.indent * tabSize);
        }

        protected override ICode VisitBlock(StmtBlock s) {
            foreach (var stmt in s.Statements) {
                this.Visit(stmt);
            }
            return s;
        }

        protected override ICode VisitAssignment(StmtAssignment s) {
            this.NewLine();
            this.Visit(s.Target);
            this.js.Append(" = ");
            this.Visit(s.Expr);
            this.js.Append(";");
            return s;
        }

        protected override ICode VisitVar(ExprVar e) {
            this.js.Append(varNames[e]);
            if (e.ExprType == Expr.NodeType.VarParameter) {
                if (!this.parameters.ContainsKey(((ExprVarParameter)e).Parameter)) {
                    this.parameters.Add(((ExprVarParameter)e).Parameter, e);
                }
            }
            // Need to visit into phi variables to check for parameters
            if (e.ExprType == Expr.NodeType.VarPhi) {
                var args = ((ExprVarPhi)e).Exprs.Where(x => x.ExprType == Expr.NodeType.VarParameter).Cast<ExprVarParameter>();
                foreach (var arg in args) {
                    if (!this.parameters.ContainsKey(arg.Parameter)) {
                        this.parameters.Add(arg.Parameter, arg);
                    }
                }
            }
            return e;
        }

        private static Dictionary<UnaryOp, string> unaryOps = new Dictionary<UnaryOp, string> {
            { UnaryOp.Not, "!" },
            { UnaryOp.Negate, "-" },
        };
        protected override ICode VisitUnary(ExprUnary e) {
            this.js.Append("(");
            this.js.Append(unaryOps[e.Op]);
            this.Visit(e.Expr);
            this.js.Append(")");
            return e;
        }

        private static Dictionary<BinaryOp, string> binaryOps = new Dictionary<BinaryOp, string> {
            { BinaryOp.Add, "+" },
            { BinaryOp.Sub, "-" },
            { BinaryOp.Mul, "*" },
            { BinaryOp.Div, "/" },
            { BinaryOp.BitwiseAnd, "&" },
            { BinaryOp.BitwiseOr, "|" },
            { BinaryOp.BitwiseXor, "^" },
            { BinaryOp.And, "&&" },
            { BinaryOp.Equal, "===" },
            { BinaryOp.NotEqual, "!=" },
            { BinaryOp.LessThan, "<" },
            { BinaryOp.LessThanOrEqual, "<=" },
            { BinaryOp.GreaterThan, ">" },
            { BinaryOp.GreaterThanOrEqual, ">=" },
            { BinaryOp.Or, "||" },
        };
        protected override ICode VisitBinary(ExprBinary e) {
            this.js.Append("(");
            this.Visit(e.Left);
            this.js.AppendFormat(" {0} ", binaryOps[e.Op]);
            this.Visit(e.Right);
            this.js.Append(")");
            return e;
        }

        protected override ICode VisitTernary(ExprTernary e) {
            this.js.Append("(");
            this.Visit(e.Condition);
            this.js.Append(" ? ");
            this.Visit(e.IfTrue);
            this.js.Append(" : ");
            this.Visit(e.IfFalse);
            this.js.Append(")");
            return e;
        }

        protected override ICode VisitIf(StmtIf s) {
            this.NewLine();
            this.js.Append("if (");
            this.Visit(s.Condition);
            this.js.Append(") {");
            this.indent++;
            this.Visit(s.Then);
            this.indent--;
            if (s.Else != null) {
                this.NewLine();
                this.js.Append("} else {");
                this.indent++;
                this.Visit(s.Else);
                this.indent--;
            }
            this.NewLine();
            this.js.Append("}");
            return s;
        }

        protected override ICode VisitDoLoop(StmtDoLoop s) {
            this.NewLine();
            this.js.Append("do {");
            this.indent++;
            this.Visit(s.Body);
            this.indent--;
            this.NewLine();
            this.js.Append("} while (");
            this.Visit(s.While);
            this.js.Append(");");
            return s;
        }

        protected override ICode VisitLiteral(ExprLiteral e) {
            if (e.Value == null) {
                this.js.Append("null");
            } else if (e.Value is string) {
                this.js.AppendFormat("\"{0}\"", e.Value);
            } else if (e.Value is char) {
                this.js.AppendFormat("'{0}'", e.Value);
            } else {
                this.js.Append(e.Value);
            }
            return e;
        }

        protected override ICode VisitReturn(StmtReturn s) {
            this.NewLine();
            this.js.Append("return");
            if (this.method.IsConstructor && !this.method.IsStatic) {
                this.js.Append(" $");
            } else if (s.Expr != null) {
                this.js.Append(" ");
                this.Visit(s.Expr);
            }

            this.js.Append(";");
            return s;
        }

        protected override ICode VisitCall(ExprCall e) {
            var callMethod = e.CallMethod;
            Action<string> appendArgs = preThis => {
                this.js.Append("(");
                var needComma = false;
                if (!callMethod.IsStatic) {
                    if (preThis != null) {
                        this.js.Append(preThis);
                    } else {
                        this.Visit(e.Obj);
                    }
                    needComma = true;
                }
                foreach (var arg in e.Args) {
                    if (needComma) {
                        this.js.Append(", ");
                    }
                    this.Visit(arg);
                    needComma = true;
                }
                this.js.Append(")");
            };
            var callResolved = this.resolver.CallResolvers.ValueOrDefault(e);
            if (callResolved != null) {
                switch (callResolved.Type) {
                case JsResolvedType.Method:
                    var methodResolved = (JsResolvedMethod)callResolved;
                    this.Visit(methodResolved.Obj);
                    this.js.Append(".");
                    this.js.Append(methodResolved.MethodName);
                    this.js.Append("(");
                    bool first = true;
                    foreach (var arg in methodResolved.Args.EmptyIfNull()) {
                        if (first) {
                            first = false;
                        } else {
                            this.js.Append(", ");
                        }
                        this.Visit(arg);
                    }
                    this.js.Append(")");
                    break;
                case JsResolvedType.Property:
                    var propertyResolved = (JsResolvedProperty)callResolved;
                    this.Visit(propertyResolved.Obj);
                    this.js.Append(".");
                    this.js.Append(propertyResolved.PropertyName);
                    break;
                default:
                    throw new NotImplementedException("Cannot handle: " + callResolved.Type);
                }
                return e;
            }
            if (callMethod.DeclaringType.IsInterface) {
                var ifaceName = this.resolver.InterfaceNames[callMethod.DeclaringType];
                var ifaceMethodIdx = this.resolver.InterfaceMethods[callMethod];
                var preThisName = this.virtualAndInterfaceCallVars.GetNewName();
                this.needVirtualCallVars.Add(preThisName);
                this.js.AppendFormat("({0} = ", preThisName);
                this.Visit(e.Obj);
                this.js.Append(")");
                this.js.AppendFormat("._.{0}[{1}]", ifaceName, ifaceMethodIdx);
                appendArgs(preThisName);
                return e;
            }
            var vTableIdx = this.resolver.VirtualCalls.ValueOrDefault(callMethod, () => -1);
            if (vTableIdx >= 0) {
                var preThisName = this.virtualAndInterfaceCallVars.GetNewName();
                this.needVirtualCallVars.Add(preThisName);
                this.js.AppendFormat("({0} = ", preThisName);
                this.Visit(e.Obj);
                this.js.Append(")");
                this.js.AppendFormat("._._[{0}]", vTableIdx);
                appendArgs(preThisName);
                return e;
            }
            var name = this.resolver.MethodNames[callMethod];
            this.js.Append(name);
            appendArgs(null);
            return e;
        }

        protected override ICode VisitFieldAccess(ExprFieldAccess e) {
            if (!e.IsStatic) {
                this.Visit(e.Obj);
                this.js.Append(".");
            }
            this.js.Append(this.resolver.FieldNames[e.Field]);
            return e;
        }

        protected override ICode VisitThis(ExprThis e) {
            this.js.Append(thisName);
            return e;
        }

        protected override ICode VisitNewObj(ExprNewObj e) {
            var name = this.resolver.MethodNames[e.CallMethod];
            this.js.Append(name);
            this.js.Append("(null");
            foreach (var arg in e.Args) {
                this.js.Append(", ");
                this.Visit(arg);
            }
            this.js.Append(")");
            return e;
        }

        protected override ICode VisitNewArray(ExprNewArray e) {
            // TODO: This does not initialise all array elements - must implement
            this.js.Append("new Array(");
            this.Visit(e.ExprNumElements);
            this.js.Append(")");
            return e;
        }

        protected override ICode VisitArrayLength(ExprArrayLength e) {
            this.Visit(e.Array);
            this.js.Append(".length");
            return e;
        }

        protected override ICode VisitVarArrayAccess(ExprVarArrayAccess e) {
            this.Visit(e.Array);
            this.js.Append("[");
            this.Visit(e.Index);
            this.js.Append("]");
            return e;
        }

        protected override ICode VisitTry(StmtTry s) {
            this.NewLine();
            this.js.Append("try {");
            this.indent++;
            this.Visit(s.Try);
            this.indent--;
            foreach (var @catch in s.Catches.EmptyIfNull()) {
                // TODO: Implement full exception processing (need some runtime type information to be able to do this)
                if (!(@catch.ExceptionObject.Type.IsObject() || @catch.ExceptionObject.Type.IsException())) {
                    throw new NotImplementedException("Cannot yet handle 'catch' of type: " + @catch.ExceptionObject.Type.Name);
                }
                this.NewLine();
                this.js.Append("} catch(");
                this.Visit(@catch.ExceptionObject);
                this.js.Append(") {");
                this.indent++;
                this.Visit(@catch.Stmt);
                this.indent--;
            }
            if (s.Finally != null) {
                this.NewLine();
                this.js.Append("} finally {");
                this.indent++;
                this.Visit(s.Finally);
                this.indent--;
            }
            this.NewLine();
            this.js.Append("}");
            return s;
        }

        protected override ICode VisitThrow(StmtThrow s) {
            this.NewLine();
            this.js.Append("throw");
            if (s.Expr != null) {
                this.js.Append(" ");
                this.Visit(s.Expr);
            }
            this.js.Append(";");
            return s;
        }

        protected override ICode VisitWrapExpr(StmtWrapExpr s) {
            this.NewLine();
            this.Visit(s.Expr);
            this.js.Append(";");
            return s;
        }

        protected override ICode VisitCast(ExprCast e) {
            this.Visit(e.Expr);
            return e;
        }

    }
}
