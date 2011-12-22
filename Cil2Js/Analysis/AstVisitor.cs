using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil;

namespace Cil2Js.Analysis {
    public abstract class AstVisitor {

        public AstVisitor() : this(false) { }

        public AstVisitor(bool throwOnNoOverride) {
            this.throwOnNoOverride = throwOnNoOverride;
        }

        private bool throwOnNoOverride;

        protected void ThrowOnNoOverride() {
            if (this.throwOnNoOverride) {
                throw new InvalidOperationException("Method not overridden");
            }
        }

        public virtual ICode Visit(ICode c) {
            if (c == null) {
                return null;
            }
            switch (c.CodeType) {
            case CodeType.Expression:
                return this.VisitExpr((Expr)c);
            case CodeType.Statement:
                return this.VisitStmt((Stmt)c);
            default:
                throw new InvalidOperationException("Invalid code type: " + c.CodeType);
            }
        }

        protected virtual ICode VisitStmt(Stmt s) {
            switch (s.StmtType) {
            case Stmt.NodeType.Cil:
                return this.VisitCil((StmtCil)s);
            case Stmt.NodeType.Continuation:
                return this.VisitContinuation((StmtContinuation)s);
            case Stmt.NodeType.Block:
                return this.VisitBlock((StmtBlock)s);
            case Stmt.NodeType.Try:
                return this.VisitTry((StmtTry)s);
            case Stmt.NodeType.Throw:
                return this.VisitThrow((StmtThrow)s);
            case Stmt.NodeType.Assignment:
                return this.VisitAssignment((StmtAssignment)s);
            case Stmt.NodeType.If:
                return this.VisitIf((StmtIf)s);
            case Stmt.NodeType.DoLoop:
                return this.VisitDoLoop((StmtDoLoop)s);
            case Stmt.NodeType.Return:
                return this.VisitReturn((StmtReturn)s);
            case Stmt.NodeType.WrapExpr:
                return this.VisitWrapExpr((StmtWrapExpr)s);
            default:
                throw new NotImplementedException("Cannot handle: " + s.StmtType);
            }
        }

        protected virtual ICode VisitCil(StmtCil s) {
            this.ThrowOnNoOverride();
            var endCil = (Stmt)this.Visit(s.EndCil);
            if (endCil != s.EndCil) {
                return new StmtCil(s.Ctx, s.Insts, endCil) {
                    StartStackSize = s.StartStackSize,
                    EndStackSize = s.EndStackSize,
                };
            } else {
                return s;
            }
        }

        protected virtual ICode VisitContinuation(StmtContinuation s) {
            throw new InvalidOperationException("Non-recursive visitor, cannot handle continuations");
        }

        protected StmtBlock HandleBlock(StmtBlock s, Func<Stmt, Stmt> fn) {
            List<Stmt> stNew = null;
            foreach (var stmt in s.Statements) {
                var o = fn(stmt);
                if (o != stmt && stNew == null) {
                    stNew = new List<Stmt>(s.Statements.TakeWhile(x => x != stmt));
                }
                if (stNew != null) {
                    stNew.Add(o);
                }
            }
            if (stNew == null) {
                return s;
            } else {
                var blocks = stNew.SelectMany(x => {
                    if (x == null) {
                        return Enumerable.Empty<Stmt>();
                    }
                    if (x.StmtType == Stmt.NodeType.Block) {
                        return ((StmtBlock)x).Statements;
                    } else {
                        return new[] { x };
                    }
                })
                .ToArray();
                return new StmtBlock(s.Ctx, blocks);
            }
        }

        protected virtual ICode VisitBlock(StmtBlock s) {
            this.ThrowOnNoOverride();
            return this.HandleBlock(s, stmt => (Stmt)this.Visit(stmt));
        }

        protected virtual ICode VisitTry(StmtTry s) {
            this.ThrowOnNoOverride();
            var @try = this.Visit(s.Try);
            List<StmtTry.Catch> catches = null;
            if (s.Catches != null) {
                foreach (var @catch in s.Catches) {
                    var stmt = (Stmt)this.Visit(@catch.Stmt);
                    var exObj = (ExprVar)this.Visit(@catch.ExceptionObject);
                    if ((stmt != @catch.Stmt || exObj != @catch.ExceptionObject) && catches == null) {
                        catches = new List<StmtTry.Catch>(s.Catches.TakeWhile(x => x != @catch));
                    }
                    if (catches != null) {
                        catches.Add(new StmtTry.Catch(stmt, exObj));
                    }
                }
            }
            var @finally = this.Visit(s.Finally);
            if (@try != s.Try || catches != null || @finally != s.Finally) {
                return new StmtTry(s.Ctx, (Stmt)@try, catches ?? s.Catches, (Stmt)@finally);
            } else {
                return s;
            }
        }

        protected virtual ICode VisitThrow(StmtThrow s) {
            this.ThrowOnNoOverride();
            var expr = this.Visit(s.Expr);
            if (expr != s.Expr) {
                return new StmtThrow(s.Ctx,(Expr)expr);
            } else {
                return s;
            }
        }

        protected virtual ICode VisitAssignment(StmtAssignment s) {
            this.ThrowOnNoOverride();
            var expr = this.Visit(s.Expr);
            var target = this.Visit(s.Target);
            if (expr != s.Expr || target != s.Target) {
                return new StmtAssignment(s.Ctx,(ExprVar)target, (Expr)expr);
            } else {
                return s;
            }
        }

        protected virtual ICode VisitIf(StmtIf s) {
            this.ThrowOnNoOverride();
            var condition = this.Visit(s.Condition);
            var then = this.Visit(s.Then);
            var @else = this.Visit(s.Else);
            if (condition != s.Condition || then != s.Then || @else != s.Else) {
                return new StmtIf(s.Ctx,(Expr)condition, (Stmt)then, (Stmt)@else);
            } else {
                return s;
            }
        }

        protected virtual ICode VisitDoLoop(StmtDoLoop s) {
            this.ThrowOnNoOverride();
            var body = this.Visit(s.Body);
            var @while = this.Visit(s.While);
            if (body != s.Body || @while != s.While) {
                return new StmtDoLoop(s.Ctx,(Stmt)body, (Expr)@while);
            } else {
                return s;
            }
        }

        protected virtual ICode VisitReturn(StmtReturn s) {
            this.ThrowOnNoOverride();
            var expr = this.Visit(s.Expr);
            return expr == s.Expr ? s : new StmtReturn(s.Ctx,(Expr)expr);
        }

        protected T HandleCall<T>(T call, Func<MethodReference, Expr, IEnumerable<Expr>, T> fnCreate) where T : ICall {
            this.ThrowOnNoOverride();
            var obj = (Expr)this.Visit(call.Obj);
            List<Expr> argsNew = null;
            foreach (var arg in call.Args) {
                var o = (Expr)this.Visit(arg);
                if (o != arg && argsNew == null) {
                    argsNew = new List<Expr>(call.Args.TakeWhile(x => x != arg));
                }
                if (argsNew != null) {
                    argsNew.Add(o);
                }
            }
            if (argsNew == null && obj == call.Obj) {
                return call;
            } else {
                return fnCreate(call.CallMethod, obj, argsNew ?? call.Args);
            }
        }

        protected virtual ICode VisitWrapExpr(StmtWrapExpr s) {
            this.ThrowOnNoOverride();
            var expr = (Expr)this.Visit(s.Expr);
            if (expr != s.Expr) {
                return new StmtWrapExpr(s.Ctx,expr);
            } else {
                return s;
            }
        }

        protected virtual ICode VisitExpr(Expr e) {
            switch (e.ExprType) {
            case Expr.NodeType.Cast:
                return this.VisitCast((ExprCast)e);
            case Expr.NodeType.NewObj:
                return this.VisitNewObj((ExprNewObj)e);
            case Expr.NodeType.FieldAccess:
                return this.VisitFieldAccess((ExprFieldAccess)e);
            case Expr.NodeType.This:
                return this.VisitThis((ExprThis)e);
            case Expr.NodeType.Call:
                return this.VisitCall((ExprCall)e);
            case Expr.NodeType.VarExprInstResult:
            case Expr.NodeType.VarLocal:
            case Expr.NodeType.VarParameter:
            case Expr.NodeType.VarPhi:
                return this.VisitVar((ExprVar)e);
            case Expr.NodeType.Literal:
                return this.VisitLiteral((ExprLiteral)e);
            case Expr.NodeType.Unary:
                return this.VisitUnary((ExprUnary)e);
            case Expr.NodeType.Binary:
                return this.VisitBinary((ExprBinary)e);
            case Expr.NodeType.Ternary:
                return this.VisitTernary((ExprTernary)e);
            case Expr.NodeType.NewArray:
                return this.VisitNewArray((ExprNewArray)e);
            case Expr.NodeType.ArrayLength:
                return this.VisitArrayLength((ExprArrayLength)e);
            case Expr.NodeType.ArrayAccess:
                return this.VisitVarArrayAccess((ExprVarArrayAccess)e);
            case Expr.NodeType.MethodReference:
                return this.VisitMethodReference((ExprMethodReference)e);
            default:
                if ((int)e.ExprType >= (int)Expr.NodeType.Max) {
                    return e;
                } else {
                    throw new NotImplementedException("Cannot handle: " + e.ExprType);
                }
            }
        }

        protected virtual ICode VisitCast(ExprCast e) {
            this.ThrowOnNoOverride();
            var expr = (Expr)this.Visit(e.Expr);
            if (expr != e.Expr) {
                return new ExprCast(e.Ctx, expr, e.Type);
            } else {
                return e;
            }
        }

        protected virtual ICode VisitNewObj(ExprNewObj e) {
            return this.HandleCall(e, (method, obj, args) => new ExprNewObj(e.Ctx, method, args));
        }

        protected virtual ICode VisitFieldAccess(ExprFieldAccess e) {
            this.ThrowOnNoOverride();
            var obj = (Expr)this.Visit(e.Obj);
            if (obj != e.Obj) {
                return new ExprFieldAccess(e.Ctx, obj, e.Field);
            } else {
                return e;
            }
        }

        protected virtual ICode VisitThis(ExprThis e) {
            this.ThrowOnNoOverride();
            return e;
        }

        protected virtual ICode VisitCall(ExprCall e) {
            return this.HandleCall(e, (method, obj, args) => new ExprCall(e.Ctx, method, obj, args, e.IsVirtualCall));
        }

        protected virtual ICode VisitVar(ExprVar e) {
            switch (e.ExprType) {
            case Expr.NodeType.VarExprInstResult:
                return this.VisitVarInstResult((ExprVarInstResult)e);
            case Expr.NodeType.VarLocal:
                return this.VisitVarLocal((ExprVarLocal)e);
            case Expr.NodeType.VarParameter:
                return this.VisitVarParameter((ExprVarParameter)e);
            case Expr.NodeType.VarPhi:
                return this.VisitVarPhi((ExprVarPhi)e);
            default:
                throw new NotImplementedException("Cannot handle: " + e.ExprType);
            }
        }

        protected virtual ICode VisitVarInstResult(ExprVarInstResult e) {
            this.ThrowOnNoOverride();
            return e;
        }

        protected virtual ICode VisitLiteral(ExprLiteral e) {
            this.ThrowOnNoOverride();
            return e;
        }

        protected virtual ICode VisitVarLocal(ExprVarLocal e) {
            this.ThrowOnNoOverride();
            return e;
        }

        protected virtual ICode VisitVarParameter(ExprVarParameter e) {
            this.ThrowOnNoOverride();
            return e;
        }

        protected virtual ICode VisitVarPhi(ExprVarPhi e) {
            this.ThrowOnNoOverride();
            List<ICode> c = null;
            int count = 0;
            foreach (var expr in e.Exprs) {
                var o = this.Visit(expr);
                if (o != expr && c == null) {
                    c = new List<ICode>(e.Exprs.Take(count));
                }
                if (c != null) {
                    c.Add(o);
                }
                count++;
            }
            if (c == null) {
                return e;
            } else {
                var exprs = c.Cast<Expr>().SelectMany(x => {
                    if (x == null) {
                        return Enumerable.Empty<Expr>();
                    }
                    if (x.ExprType == Expr.NodeType.VarPhi) {
                        return ((ExprVarPhi)x).Exprs;
                    } else {
                        return new[] { x };
                    }
                })
                .ToArray();
                return new ExprVarPhi(e.Ctx) { Exprs = exprs };
            }
        }

        protected virtual ICode VisitUnary(ExprUnary e) {
            this.ThrowOnNoOverride();
            var expr = this.Visit(e.Expr);
            if (expr != e.Expr) {
                return new ExprUnary(e.Ctx, e.Op, e.Type, (Expr)expr);
            } else {
                return e;
            }
        }

        protected virtual ICode VisitBinary(ExprBinary e) {
            this.ThrowOnNoOverride();
            var left = this.Visit(e.Left);
            var right = this.Visit(e.Right);
            if (left == e.Left && right == e.Right) {
                return e;
            } else {
                return new ExprBinary(e.Ctx, e.Op, e.Type, (Expr)left, (Expr)right);
            }
        }

        protected virtual ICode VisitTernary(ExprTernary e) {
            this.ThrowOnNoOverride();
            var condition = (Expr)this.Visit(e.Condition);
            var ifTrue = (Expr)this.Visit(e.IfTrue);
            var ifFalse = (Expr)this.Visit(e.IfFalse);
            if (condition != e.Condition || ifTrue != e.IfTrue || ifFalse != e.IfFalse) {
                return new ExprTernary(e.Ctx, condition, ifTrue, ifFalse);
            } else {
                return e;
            }
        }

        protected virtual ICode VisitNewArray(ExprNewArray e) {
            this.ThrowOnNoOverride();
            var numElements = (Expr)this.Visit(e.ExprNumElements);
            if (numElements != e.ExprNumElements) {
                return new ExprNewArray(e.Ctx, e.Type.GetElementType(), numElements);
            } else {
                return e;
            }
        }

        protected virtual ICode VisitArrayLength(ExprArrayLength e) {
            this.ThrowOnNoOverride();
            var array = (Expr)this.Visit(e.Array);
            if (array != e.Array) {
                return new ExprArrayLength(e.Ctx, array);
            } else {
                return e;
            }
        }

        protected virtual ICode VisitVarArrayAccess(ExprVarArrayAccess e) {
            this.ThrowOnNoOverride();
            var array = (Expr)this.Visit(e.Array);
            var index = (Expr)this.Visit(e.Index);
            if (array != e.Array || index != e.Index) {
                return new ExprVarArrayAccess(e.Ctx, array, index);
            } else {
                return e;
            }
        }

        protected virtual ICode VisitMethodReference(ExprMethodReference e) {
            return e;
        }

    }
}
