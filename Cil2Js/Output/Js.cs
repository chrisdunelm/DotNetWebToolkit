using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Analysis;
using Cil2Js.Ast;
using Mono.Cecil;
using Cil2Js.Utils;

namespace Cil2Js.Output {
    public class Js : AstVisitor {

        class VarNamer : AstVisitor {

            class VarClustered : ExprVar {

                public VarClustered(IEnumerable<ExprVar> vars) {
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

            private readonly static char[] c = "abcdefghijklmnopqrstuvwxyz".ToArray();

            public static Dictionary<ExprVar, string> V(ICode c) {
                var v = new VarNamer();
                v.Visit(c);
                var clusters = UniqueClusters(v.clusters);
                var allCounts = clusters.ToDictionary(x => (ExprVar)new VarClustered(x), x => x.Sum(y => v.varCount.ValueOrDefault(y)));
                var allClusteredCounts = allCounts.ToArray();
                var allClustered = new HashSet<ExprVar>(clusters.SelectMany(x => x));
                foreach (var varCount in v.varCount) {
                    if (!allClustered.Contains(varCount.Key)) {
                        allCounts.Add(varCount.Key, varCount.Value);
                    }
                }
                var names = allCounts
                    .OrderByDescending(x => x.Value)
                    .ToDictionary(x => x.Key, x => v.GetNewName());
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

            private VarNamer() { }

            private Dictionary<ExprVar, int> varCount = new Dictionary<ExprVar, int>();
            private List<ExprVar[]> clusters = new List<ExprVar[]>();

            private int curName = 0;
            private string GetNewName() {
                var l = c.Length;
                int length = 1, sub = 0;
                for (int i = l, add = l; ; sub += add, add *= l, i += add, length++) {
                    if (this.curName < i) {
                        break;
                    }
                }
                var v = this.curName - sub;
                string name = "";
                for (int i = 0; i < length; i++) {
                    name += c[v % l];
                    v = v / l;
                }
                this.curName++;
                name = new string(name.Reverse().ToArray());
                return name;
            }

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

        public static string Create(MethodDefinition method, string methodName, Func<MethodReference, string> methodNameResolver, ICode ast) {
            if (methodNameResolver == null) {
                methodNameResolver = mr => mr.Name;
            }
            var varNames = VarNamer.V(ast);
            var v = new Js(varNames, methodNameResolver);
            v.Visit(ast);
            var js = v.js.ToString();
            var sb = new StringBuilder();
            var parameterNames = method.Parameters.Select(x => v.parameters.ValueOrDefault(x, () => null).NullThru(y => varNames[y], "_")).ToArray();
            sb.AppendFormat("function {0}({1}) {{", methodName, string.Join(", ", parameterNames));
            var vars = varNames
                .Select(x => x.Value)
                .Except(parameterNames)
                .Distinct()
                .ToArray();
            if (vars.Any()) {
                sb.AppendLine();
                sb.Append(' ', tabSize);
                sb.AppendFormat("var {0};", string.Join(", ", vars.Select(x => x)));
            }
            sb.AppendLine(js);
            sb.AppendLine("}");
            return sb.ToString();
        }

        private Js(Dictionary<ExprVar, string> varNames, Func<MethodReference, string> methodNameResolver)
            : base(true) {
            this.varNames = varNames;
            this.methodNameResolver = methodNameResolver;
        }

        private Dictionary<ExprVar, string> varNames;
        private Func<MethodReference, string> methodNameResolver;

        private Dictionary<ParameterDefinition, ExprVar> parameters = new Dictionary<ParameterDefinition, ExprVar>();
        private StringBuilder js = new StringBuilder();

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
            { BinaryOp.And, "&&" },
            { BinaryOp.Equal, "==" },
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
            } else {
                this.js.Append(e.Value);
            }
            return e;
        }

        protected override ICode VisitReturn(StmtReturn s) {
            this.NewLine();
            this.js.Append("return");
            if (s.Expr != null) {
                this.js.Append(" ");
                this.Visit(s.Expr);
            }
            this.js.Append(";");
            return s;
        }

        private void VisitCall(ICall call) {
            var name = this.methodNameResolver(call.Calling);
            this.js.Append(name);
            this.js.Append("(");
            foreach (var arg in call.Args) {
                this.Visit(arg);
                this.js.Append(", ");
            }
            this.js.Length -= 2;
            this.js.Append(")");
        }

        protected override ICode VisitCall(ExprCall e) {
            this.VisitCall(e);
            return e;
        }

        protected override ICode VisitCall(StmtCall s) {
            this.NewLine();
            this.VisitCall(s);
            this.js.Append(";");
            return s;
        }

    }
}
