using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Analysis;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;

namespace DotNetWebToolkit.Cil2Js.Utils {
    public class ShowVisitor : JsAstVisitor {

        private static string GetStmtName(Stmt s) {
            if (s.StmtType == Stmt.NodeType.Cil) {
                var sCil = (StmtCil)s;
                if (sCil.Insts != null && sCil.Insts.Any()) {
                    return string.Format("IL_{0:x4}", sCil.Insts.First().Offset);
                }
            }
            return string.Format("{0:x8}", s.GetHashCode());
        }

        public static string V(ICode c) {
            var method = c.Ctx.MRef;
            StringBuilder sb = new StringBuilder();
            sb.Append(method.FullName);
            var seen = new HashSet<ICode>() { c };
            var todo = new Queue<Stmt>();
            todo.Enqueue((Stmt)c);
            while (todo.Any()) {
                var cBlock = todo.Dequeue();
                var v = new ShowVisitor();
                v.Visit(cBlock);
                sb.AppendLine();
                sb.Append(GetStmtName(cBlock) + ":");
                sb.Append(v.Code);
                foreach (var continuation in v.Continuations) {
                    if (seen.Add(continuation.To)) {
                        todo.Enqueue(continuation.To);
                    }
                }
            }

            sb.AppendLine();
            sb.Append("}");
            return sb.ToString();
        }

        private ShowVisitor()
            : base(true) {
        }

        private StringBuilder code = new StringBuilder();
        private int indent = 1;
        private List<StmtContinuation> continuations = new List<StmtContinuation>();

        public string Code { get { return this.code.ToString(); } }
        public IEnumerable<StmtContinuation> Continuations { get { return this.continuations; } }


        private void NewLine() {
            this.code.Append(Environment.NewLine);
            this.code.Append(' ', this.indent * 4);
        }

        protected override ICode VisitCil(StmtCil s) {
            this.NewLine();
            this.code.Append("CIL: " + s.ToString());
            this.Visit(s.EndCil);
            return s;
        }

        protected override ICode VisitBlock(StmtBlock s) {
            foreach (var stmt in s.Statements) {
                this.Visit(stmt);
            }
            return s;
        }

        protected override ICode VisitIf(StmtIf s) {
            this.NewLine();
            this.code.Append("if (");
            this.Visit(s.Condition);
            this.code.Append(") {");
            this.indent++;
            this.Visit(s.Then);
            this.indent--;
            if (s.Else != null) {
                this.NewLine();
                this.code.Append("} else {");
                this.indent++;
                this.Visit(s.Else);
                this.indent--;
            }
            this.NewLine();
            this.code.Append("}");
            return s;
        }

        protected override ICode VisitContinuation(StmtContinuation s) {
            this.NewLine();
            this.code.AppendFormat("-> {0}", GetStmtName(s.To));
            if (s.LeaveProtectedRegion) {
                this.code.Append(" [leave protected region]");
            }
            this.continuations.Add(s);
            return s;
        }

        private static Dictionary<UnaryOp, string> unaryOps = new Dictionary<UnaryOp, string> {
            { UnaryOp.Not, "!" },
            { UnaryOp.Negate, "-" },
        };
        protected override ICode VisitUnary(ExprUnary e) {
            this.code.Append("(");
            this.code.Append(unaryOps[e.Op]);
            this.Visit(e.Expr);
            this.code.Append(")");
            return e;
        }

        protected override ICode VisitReturn(StmtReturn s) {
            this.NewLine();
            this.code.Append("return");
            if (s.Expr != null) {
                this.code.Append(" ");
                this.Visit(s.Expr);
            }
            return s;
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
            { BinaryOp.Equal, "==" },
            { BinaryOp.NotEqual, "!=" },
            { BinaryOp.LessThan, "<" },
            { BinaryOp.LessThanOrEqual, "<=" },
            { BinaryOp.GreaterThan, ">" },
            { BinaryOp.GreaterThanOrEqual, ">=" },
            { BinaryOp.Or, "||" },
        };
        protected override ICode VisitBinary(ExprBinary e) {
            this.code.Append("(");
            this.Visit(e.Left);
            this.code.AppendFormat(" {0} ", binaryOps[e.Op]);
            this.Visit(e.Right);
            this.code.Append(")");
            return e;
        }

        protected override ICode VisitTernary(ExprTernary e) {
            this.code.Append("(");
            this.Visit(e.Condition);
            this.code.Append(" ? ");
            this.Visit(e.IfTrue);
            this.code.Append(" : ");
            this.Visit(e.IfFalse);
            this.code.Append(")");
            return e;
        }

        protected override ICode VisitVarInstResult(ExprVarInstResult e) {
            this.code.Append(e.ToString());
            return e;
        }

        protected override ICode VisitLiteral(ExprLiteral e) {
            if (e.Value == null) {
                this.code.Append("null");
            } else if (e.Type.IsString()) {
                this.code.Append("\"" + e.Value + "\"");
            } else if (e.Type.IsChar()) {
                this.code.Append("'" + e.Value + "'");
            } else {
                this.code.Append(e.Value);
            }
            return e;
        }

        protected override ICode VisitAssignment(StmtAssignment s) {
            this.NewLine();
            this.Visit(s.Target);
            this.code.Append(" = ");
            this.Visit(s.Expr);
            return s;
        }

        protected override ICode VisitVarLocal(ExprVarLocal e) {
            this.code.Append(e.ToString());
            return e;
        }

        protected override ICode VisitVarParameter(ExprVarParameter e) {
            this.code.Append(e.ToString());
            return e;
        }

        private HashSet<ExprVarPhi> phiSeen = null;
        protected override ICode VisitVarPhi(ExprVarPhi e) {
            bool finalise = false;
            if (this.phiSeen == null) {
                this.phiSeen = new HashSet<ExprVarPhi>();
                finalise = true;
            }
            this.code.AppendFormat("phi<{0}>(", e.GetHashCode());
            if (this.phiSeen.Add(e)) {
                foreach (var v in e.Exprs) {
                    this.Visit(v);
                    this.code.Append(",");
                }
                this.code.Length--;
            } else {
                this.code.AppendFormat("<rec-{0}>", e.GetHashCode());
            }
            this.code.Append(")");
            if (finalise) {
                this.phiSeen = null;
            }
            return e;
        }

        protected override ICode VisitDoLoop(StmtDoLoop s) {
            this.NewLine();
            this.code.Append("do {");
            this.indent++;
            this.Visit(s.Body);
            this.indent--;
            this.NewLine();
            this.code.Append("} while (");
            this.Visit(s.While);
            this.code.Append(")");
            return s;
        }

        private void VisitCall(ICall call, bool isConstructor) {
            var method = call.CallMethod;
            if (isConstructor) {
                this.code.Append(method.DeclaringType.Name);
            } else if (!method.HasThis) {
                this.code.Append(method.DeclaringType.Name + "." + method.Name);
            } else {
                this.Visit(call.Obj);
                this.code.Append(".");
                this.code.Append(method.Name);
            }
            this.code.Append("(");
            if (call.Args.Any()) {
                foreach (var arg in call.Args) {
                    this.Visit(arg);
                    this.code.Append(", ");
                }
                this.code.Length -= 2;
            }
            this.code.Append(")");
        }

        protected override ICode VisitCall(ExprCall e) {
            this.VisitCall((ICall)e, false);
            return e;
        }

        protected override ICode VisitNewObj(ExprNewObj e) {
            this.code.AppendFormat("new ");
            this.VisitCall(e, true);
            return e;
        }

        protected override ICode VisitFieldAccess(ExprFieldAccess e) {
            if (e.IsStatic) {
                this.code.Append(e.Field.DeclaringType.FullName);
            } else {
                this.Visit(e.Obj);
            }
            this.code.Append(".");
            this.code.Append(e.Field.Name);
            return e;
        }

        protected override ICode VisitVarThis(ExprVarThis e) {
            this.code.Append("this");
            return e;
        }

        protected override ICode VisitWrapExpr(StmtWrapExpr s) {
            this.NewLine();
            this.Visit(s.Expr);
            this.code.Append(";");
            return s;
        }

        protected override ICode VisitNewArray(ExprNewArray e) {
            this.code.Append("new[");
            this.Visit(e.ExprNumElements);
            this.code.Append("]");
            return e;
        }

        protected override ICode VisitVarArrayAccess(ExprVarArrayAccess e) {
            this.Visit(e.Array);
            this.code.Append("[");
            this.Visit(e.Index);
            this.code.Append("]");
            return e;
        }

        protected override ICode VisitArrayLength(ExprArrayLength e) {
            this.Visit(e.Array);
            this.code.Append(".Length");
            return e;
        }

        protected override ICode VisitCast(ExprCast e) {
            this.code.AppendFormat("({0})", e.Type);
            this.Visit(e.Expr);
            return e;
        }

        protected override ICode VisitIsInst(ExprIsInst e) {
            this.code.AppendFormat("({0}?)", e.Type);
            this.Visit(e.Expr);
            return e;
        }

        protected override ICode VisitThrow(StmtThrow s) {
            this.NewLine();
            this.code.Append("throw");
            if (s.Expr != null) {
                this.code.Append(" ");
                this.Visit(s.Expr);
            }
            return s;
        }

        protected override ICode VisitTry(StmtTry s) {
            this.NewLine();
            this.code.Append("try {");
            this.indent++;
            this.Visit(s.Try);
            this.indent--;
            foreach (var @catch in s.Catches.EmptyIfNull()) {
                this.NewLine();
                this.code.Append("} catch (");
                this.Visit(@catch.ExceptionVar);
                this.code.Append(") {");
                this.indent++;
                this.Visit(@catch.Stmt);
                this.indent--;
            }
            if (s.Finally != null) {
                this.NewLine();
                this.code.Append("} finally {");
                this.indent++;
                this.Visit(s.Finally);
                this.indent--;
            }
            this.NewLine();
            this.code.Append("}");
            return s;
        }

        protected override ICode VisitMethodReference(ExprMethodReference e) {
            this.code.AppendFormat("&{0}", e.Method.Name);
            return e;
        }

        protected override ICode VisitSwitch(StmtSwitch s) {
            this.NewLine();
            this.code.Append("switch (");
            this.Visit(s.Expr);
            this.code.Append(") {");
            foreach (var @case in s.Cases) {
                this.NewLine();
                this.code.AppendFormat("case {0}:", @case.Value);
                this.indent++;
                this.Visit(@case.Stmt);
                this.indent--;
            }
            if (s.Default != null) {
                this.NewLine();
                this.code.Append("default:");
                this.indent++;
                this.Visit(s.Default);
                this.indent--;
            }
            this.NewLine();
            this.code.Append("}");
            return s;
        }

        protected override ICode VisitBreak(StmtBreak s) {
            this.NewLine();
            this.code.Append("break");
            return s;
        }

        protected override ICode VisitEmpty(StmtEmpty s) {
            return s;
        }

        protected override ICode VisitBox(ExprBox e) {
            this.code.Append("box(");
            this.Visit(e.Expr);
            this.code.Append(")");
            return e;
        }

        protected override ICode VisitUnbox(ExprUnbox e) {
            this.code.Append("unbox(");
            this.Visit(e.Expr);
            this.code.Append(")");
            return e;
        }

        protected override ICode VisitDefaultValue(ExprDefaultValue e) {
            this.code.AppendFormat("default({0})", e.Type.Name);
            return e;
        }

        protected override ICode VisitRuntimeHandle(ExprRuntimeHandle e) {
            this.code.AppendFormat("{0}<{1}>", e.Type.Name, e.Member.Name);
            return e;
        }

    }
}
