using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Analysis;
using Cil2Js.Ast;
using Mono.Cecil;
using Cil2Js.Utils;

namespace Cil2Js.Output {
    public class JsMethod : JsAstVisitor {

        public class Resolver {
            public Dictionary<ExprVar, string> LocalVarNames { get; set; }
            public Dictionary<MethodReference, string> MethodNames { get; set; }
            public Dictionary<FieldReference, string> FieldNames { get; set; }
            public Dictionary<TypeReference, string> TypeNames { get; set; }
        }

        //public class Resolver {

        //    public Resolver(
        //        IDictionary<MethodDefinition, string> methodNames,
        //        IDictionary<FieldDefinition, string> fieldNames,
        //        IDictionary<TypeDefinition, string> vTables,
        //        IDictionary<MethodDefinition, int> virtualCalls,
        //        IDictionary<ICall, JsResolved> callResolvers,
        //        IDictionary<MethodDefinition, IEnumerable<MethodDefinition>> cctorsCalled,
        //        IDictionary<TypeDefinition, string> interfaceNames,
        //        IDictionary<MethodDefinition, int> interfaceMethods,
        //        IDictionary<TypeDefinition, Dictionary<TypeDefinition, string>> interfaceCallsNames) {
        //        this.MethodNames = methodNames;
        //        this.FieldNames = fieldNames;
        //        this.VTables = vTables;
        //        this.VirtualCalls = virtualCalls;
        //        this.CallResolvers = callResolvers;
        //        this.CctorsCalled = cctorsCalled;
        //        this.InterfaceNames = interfaceNames;
        //        this.InterfaceMethods = interfaceMethods;
        //        this.InterfaceCallsNames = interfaceCallsNames;
        //    }

        //    public IDictionary<MethodDefinition, string> MethodNames { get; private set; }
        //    public IDictionary<FieldDefinition, string> FieldNames { get; private set; }
        //    public IDictionary<TypeDefinition, string> VTables { get; private set; }
        //    public IDictionary<MethodDefinition, int> VirtualCalls { get; private set; }
        //    public IDictionary<ICall, JsResolved> CallResolvers { get; private set; }
        //    public IDictionary<MethodDefinition, IEnumerable<MethodDefinition>> CctorsCalled { get; private set; }
        //    public IDictionary<TypeDefinition, string> InterfaceNames { get; private set; }
        //    public IDictionary<MethodDefinition, int> InterfaceMethods { get; private set; }
        //    public IDictionary<TypeDefinition, Dictionary<TypeDefinition, string>> InterfaceCallsNames { get; private set; }

        //}

        //class LocalVarNamer : JsAstVisitor {

        //    class VarClustered : ExprVar {

        //        public VarClustered(Ctx ctx, IEnumerable<ExprVar> vars)
        //            : base(ctx) {
        //            this.Vars = vars;
        //        }

        //        public IEnumerable<ExprVar> Vars { get; private set; }

        //        public override Expr.NodeType ExprType {
        //            get { throw new NotImplementedException(); }
        //        }

        //        public override TypeReference Type {
        //            get { return this.Vars.Select(x => x.Type).Aggregate((a, b) => TypeCombiner.Combine(this.Ctx, a, b)); }
        //        }
        //    }

        //    public static Dictionary<ExprVar, string> V(ICode c) {
        //        var v = new LocalVarNamer();
        //        v.Visit(c);
        //        var clusters = UniqueClusters(v.clusters);
        //        var allCounts = clusters.ToDictionary(x => (ExprVar)new VarClustered(c.Ctx, x), x => x.Sum(y => v.varCount.ValueOrDefault(y)));
        //        var allClusteredCounts = allCounts.ToArray();
        //        var allClustered = new HashSet<ExprVar>(clusters.SelectMany(x => x));
        //        foreach (var varCount in v.varCount) {
        //            if (!allClustered.Contains(varCount.Key)) {
        //                allCounts.Add(varCount.Key, varCount.Value);
        //            }
        //        }
        //        var nameGenerator = new NameGenerator();
        //        var names = allCounts
        //            .OrderByDescending(x => x.Value)
        //            .ToDictionary(x => x.Key, x => nameGenerator.GetNewName());
        //        foreach (var cluster in allClusteredCounts) {
        //            foreach (var var in ((VarClustered)cluster.Key).Vars) {
        //                names.Add(var, names[cluster.Key]);
        //            }
        //        }
        //        return names;
        //    }

        //    private static IEnumerable<ExprVar[]> UniqueClusters(IEnumerable<ExprVar[]> clusters) {
        //    start:
        //        foreach (var a in clusters) {
        //            foreach (var b in clusters) {
        //                if (a == b) {
        //                    continue;
        //                }
        //                if (a.Intersect(b).Any()) {
        //                    clusters = clusters.Where(x => x != a && x != b).Concat(a.Union(b).ToArray()).ToArray();
        //                    goto start;
        //                }
        //            }
        //        }
        //        return clusters;
        //    }

        //    private LocalVarNamer() { }

        //    private Dictionary<ExprVar, int> varCount = new Dictionary<ExprVar, int>();
        //    private List<ExprVar[]> clusters = new List<ExprVar[]>();

        //    protected override ICode VisitVar(ExprVar e) {
        //        if (e.ExprType == Expr.NodeType.VarPhi) {
        //            var ePhi = (ExprVarPhi)e;
        //            this.clusters.Add(ePhi.Exprs.Cast<ExprVar>().Concat(ePhi).ToArray());
        //        }
        //        var curCount = this.varCount.ValueOrDefault(e, () => 0);
        //        this.varCount[e] = curCount + 1;
        //        return e;
        //    }

        //}

        private const int tabSize = 4;
        //private const string thisName = "$";

        public static string Create(MethodReference mRef, Resolver resolver, ICode ast) {
            var mDef = mRef.Resolve();
            if (mDef.IsAbstract) {
                throw new ArgumentException("Cannot transcode an abstract method");
            }
            if (mDef.IsInternalCall) {
                throw new ArgumentException("Cannot transcode an internal method");
            }

            var v = new JsMethod(mDef, resolver);
            v.Visit(ast);
            var js = v.js.ToString();

            var sb = new StringBuilder();
            // Method declaration
            var methodName = resolver.MethodNames[mRef];
            var pNames = mRef.Parameters.Select(x => v.parameters.ValueOrDefault(x).NullThru(y => resolver.LocalVarNames[y], "_")).ToArray();
            if (!mDef.IsStatic) {
                var thisName = v.vars.FirstOrDefault(x => x.ExprType == Expr.NodeType.VarThis).NullThru(x => resolver.LocalVarNames[x], "_");
                pNames = pNames.Prepend(thisName).ToArray();
            }
            sb.AppendFormat("var {0} = function({1}) {{", methodName, string.Join(", ", pNames));
            // Variable declerations
            var declVars = v.vars
                .Select(x => new { name = resolver.LocalVarNames[x], type = x.Type })
                .Where(x => !pNames.Contains(x.name))
                .Select(x => x.name + " = " + DefaultValuer.Get(x.type))
                .Distinct() // Bit of a hack, but works for now
                .ToArray();
            if (declVars.Any()) {
                sb.AppendLine();
                sb.Append(' ', tabSize);
                sb.AppendFormat("var {0};", string.Join(", ", declVars));
            }

            // Method body
            sb.AppendLine(js);
            // Method ending
            sb.AppendLine("}");

            var sbStr = sb.ToString();
            return sbStr;
            //var parameterNames = mDef.Parameters.Select(x => v.parameters.ValueOrDefault(x, () => null).NullThru(y => varNames[y], "_")).ToArray();
            //if (!mDef.IsStatic) {
            //    parameterNames = new[] { thisName }.Concat(parameterNames).ToArray();
            //}
            //var fnFmt = mDef.IsConstructor && mDef.IsStatic ? "var {0} = function({1})" : "function {0}({1})";
            //sb.AppendFormat(fnFmt + " {{", methodName, string.Join(", ", parameterNames));
            //var vars = varNames
            //    .Where(x => !parameterNames.Contains(x.Value))
            //    .Select(x => x.Value + " = " + DefaultValuer.Get(x.Key.Type))
            //    .Distinct()
            //    .Concat(v.needVirtualCallVars)
            //    .ToArray();
            //if (vars.Any()) {
            //    sb.AppendLine();
            //    sb.Append(' ', tabSize);
            //    sb.AppendFormat("var {0};", string.Join(", ", vars.Select(x => x)));
            //}

            //if (mDef.IsConstructor && mDef.IsStatic) {
            //    // If this is a cctor, then mark it as called
            //    sb.AppendLine();
            //    sb.Append(' ', tabSize);
            //    sb.AppendFormat("{0} = null;", methodName);
            //}
            //// Make sure that static constructors are called before any references to static members
            //foreach (var cctor in resolver.CctorsCalled[mDef]) {
            //    if (cctor != mDef) {
            //        // Don't re-call same cctor!
            //        sb.AppendLine();
            //        sb.Append(' ', tabSize);
            //        sb.AppendFormat("if ({0}) {0}();", resolver.MethodNames[cctor]);
            //    }
            //}

            //if (mDef.IsConstructor && !mDef.IsStatic && !mDef.DeclaringType.IsAbstract) {
            //    // In a non-static constructor, the object with type information must be constructed
            //    sb.AppendLine();
            //    sb.Append(' ', tabSize);
            //    sb.AppendFormat("if (!{0}) {0} = {{_:{{", thisName);
            //    bool needComma = false;
            //    var vTableName = resolver.VTables.ValueOrDefault(mDef.DeclaringType);
            //    if (vTableName != null) {
            //        sb.AppendFormat("_:{0}", vTableName);
            //        needComma = true;
            //    }
            //    var ifaces = mDef.DeclaringType.GetAllInterfaces().Where(x => resolver.InterfaceNames.ContainsKey(x)).ToArray();
            //    foreach (var iface in ifaces) {
            //        if (needComma) {
            //            sb.Append(", ");
            //        } else {
            //            needComma = true;
            //        }
            //        sb.AppendFormat("{0}:{1}", resolver.InterfaceNames[iface], resolver.InterfaceCallsNames[mDef.DeclaringType][iface]);
            //    }
            //    sb.Append("}};");
            //    // All fields must be initialised
            //    // TODO: Does not work with fields that have generic types
            //   /* foreach (var field in method.DeclaringType.Fields.Where(x => resolver.FieldNames.ContainsKey(x))) {
            //        sb.AppendLine();
            //        sb.Append(' ', tabSize);
            //        sb.AppendFormat("{0}.{1} = {2};", thisName,
            //            resolver.FieldNames[field], DefaultValuer.Get(field.GetResolvedType()));
            //    }*/
            //}

            //sb.AppendLine(js);
            //sb.AppendLine("}");
            //return sb.ToString();
        }

        private JsMethod(MethodDefinition method, Resolver resolver)
            : base(true) {
            this.method = method;
            this.resolver = resolver;
        }

        private MethodDefinition method;
        private Resolver resolver;

        private Dictionary<ParameterDefinition, ExprVar> parameters = new Dictionary<ParameterDefinition, ExprVar>();
        private StringBuilder js = new StringBuilder();
        private List<ExprVar> vars = new List<ExprVar>();
        //private List<string> needVirtualCallVars = new List<string>();

        //private NameGenerator virtualAndInterfaceCallVars = new NameGenerator("__");
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
            this.js.Append(this.resolver.LocalVarNames[e]);
            this.vars.Add(e);
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

        protected override ICode VisitSwitch(StmtSwitch s) {
            this.NewLine();
            this.js.Append("switch (");
            this.Visit(s.Expr);
            this.js.Append(") {");
            foreach (var @case in s.Cases) {
                this.NewLine();
                this.js.AppendFormat("case {0}:", @case.Value);
                this.indent++;
                this.Visit(@case.Stmt);
                this.indent--;
            }
            if (s.Default != null) {
                this.NewLine();
                this.js.Append("default:");
                this.indent++;
                this.Visit(s.Default);
                this.indent--;
            }
            this.NewLine();
            this.js.Append("}");
            return s;
        }

        protected override ICode VisitBreak(StmtBreak s) {
            this.NewLine();
            this.js.Append("break;");
            return s;
        }

        protected override ICode VisitLiteral(ExprLiteral e) {
            if (e.Value == null) {
                this.js.Append("null");
            } else if (e.Value is string) {
                this.js.AppendFormat("\"{0}\"", e.Value);
            } else if (e.Value is char) {
                this.js.AppendFormat("'{0}'", e.Value);
            } else if (e.Type.IsBoolean()) {
                this.js.Append((bool)e.Value ? "true" : "false");
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
            var mRef = e.CallMethod;
            var mDef = mRef.Resolve();
            //throw new NotImplementedException();
            //var callMethod = e.CallMethod.Resolve();
            Action appendArgs = () => {
                var args = e.Args.ToArray();
                if (!mDef.IsStatic) {
                    args = args.Prepend(e.Obj).ToArray();
                }
                this.js.Append("(");
                for (int i = 0; i < args.Length; i++) {
                    if (i != 0) {
                        this.js.Append(", ");
                    }
                    this.Visit(args[i]);
                }
                this.js.Append(")");
            };
            //Action<string> appendArgs = preThis => {
            //    this.js.Append("(");
            //    var needComma = false;
            //    if (!callMethod.IsStatic) {
            //        if (preThis != null) {
            //            this.js.Append(preThis);
            //        } else {
            //            this.Visit(e.Obj);
            //        }
            //        needComma = true;
            //    }
            //    foreach (var arg in e.Args) {
            //        if (needComma) {
            //            this.js.Append(", ");
            //        }
            //        this.Visit(arg);
            //        needComma = true;
            //    }
            //    this.js.Append(")");
            //};
            //var callResolved = this.resolver.CallResolvers.ValueOrDefault(e);
            //if (callResolved != null) {
            //    switch (callResolved.Type) {
            //    case JsResolvedType.Method:
            //        var methodResolved = (JsResolvedMethod)callResolved;
            //        if (methodResolved.Obj != null) {
            //            this.Visit(methodResolved.Obj);
            //            this.js.Append(".");
            //        }
            //        this.js.Append(methodResolved.MethodName);
            //        this.js.Append("(");
            //        bool first = true;
            //        foreach (var arg in methodResolved.Args.EmptyIfNull()) {
            //            if (first) {
            //                first = false;
            //            } else {
            //                this.js.Append(", ");
            //            }
            //            this.Visit(arg);
            //        }
            //        this.js.Append(")");
            //        break;
            //    case JsResolvedType.Property:
            //        var propertyResolved = (JsResolvedProperty)callResolved;
            //        this.Visit(propertyResolved.Obj);
            //        this.js.Append(".");
            //        this.js.Append(propertyResolved.PropertyName);
            //        if (e.Args.Count() == 1) {
            //            // Setter of property, so emit the ' = ...'
            //            this.js.Append(" = ");
            //            this.Visit(e.Args.First());
            //        }
            //        break;
            //    default:
            //        throw new NotImplementedException("Cannot handle: " + callResolved.Type);
            //    }
            //    return e;
            //}
            //if (callMethod.DeclaringType.IsInterface) {
            //    var ifaceName = this.resolver.InterfaceNames[callMethod.DeclaringType];
            //    var ifaceMethodIdx = this.resolver.InterfaceMethods[callMethod];
            //    var preThisName = this.virtualAndInterfaceCallVars.GetNewName();
            //    this.needVirtualCallVars.Add(preThisName);
            //    this.js.AppendFormat("({0} = ", preThisName);
            //    this.Visit(e.Obj);
            //    this.js.Append(")");
            //    this.js.AppendFormat("._.{0}[{1}]", ifaceName, ifaceMethodIdx);
            //    appendArgs(preThisName);
            //    return e;
            //}
            //var vTableIdx = this.resolver.VirtualCalls.ValueOrDefault(callMethod, () => -1);
            //if (vTableIdx >= 0) {
            //    var preThisName = this.virtualAndInterfaceCallVars.GetNewName();
            //    this.needVirtualCallVars.Add(preThisName);
            //    this.js.AppendFormat("({0} = ", preThisName);
            //    this.Visit(e.Obj);
            //    this.js.Append(")");
            //    this.js.AppendFormat("._._[{0}]", vTableIdx);
            //    appendArgs(preThisName);
            //    return e;
            //}
            if (mDef.DeclaringType.IsInterface) {
                throw new NotImplementedException("Cannot handle interface calls");
            }
            if (e.IsVirtualCall) {
                throw new NotImplementedException("Cannot handle virtual calls");
            }
            var name = this.resolver.MethodNames[mRef];
            this.js.Append(name);
            appendArgs();
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

        protected override ICode VisitVarThis(ExprVarThis e) {
            this.js.Append(this.resolver.LocalVarNames[e]);
            return e;
        }

        protected override ICode VisitNewObj(ExprNewObj e) {
            var name = this.resolver.MethodNames[e.CallMethod];
            this.js.Append(name);
            this.js.Append("({_:");
            this.js.Append(this.resolver.TypeNames[e.Type]);
            this.js.Append("}");
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

        protected override ICode VisitJsFunction(ExprJsFunction e) {
            this.js.Append("(function(");//) { ");
            bool needComma = false;
            foreach (var arg in e.Args) {
                if (needComma) {
                    this.js.Append(", ");
                } else {
                    needComma = true;
                }
                this.Visit(arg);
            }
            this.js.Append(") {");
            this.indent++;
            this.Visit(e.Body);
            this.indent--;
            this.NewLine();
            this.js.Append("})");
            return e;
        }

        protected override ICode VisitJsInvoke(ExprJsInvoke e) {
            this.Visit(e.MethodToInvoke);
            this.js.Append("(");
            bool needComma = false;
            foreach (var arg in e.Args) {
                if (needComma) {
                    this.js.Append(", ");
                } else {
                    needComma = true;
                }
                this.Visit(arg);
            }
            this.js.Append(")");
            return e;
        }

    }
}
