using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Cil2Js.Ast;
using Mono.Cecil.Cil;

namespace Cil2Js.Analysis {
    class CilProcessor {

        public CilProcessor(MethodDefinition method, Stack<Expr> stack, Expr[] locals, Expr[] args, Dictionary<Instruction, ExprVarInstResult> instResults) {
            this.method = method;
            this.stack = stack;
            this.locals = locals;
            this.args = args;
            this.instResults = instResults;
            this.typeSystem = method.Module.TypeSystem;
        }

        private MethodDefinition method;
        private Stack<Expr> stack;
        private Expr[] locals, args;
        private Dictionary<Instruction, ExprVarInstResult> instResults;
        private TypeSystem typeSystem;

        public Stmt Process(Instruction inst) {
            switch (inst.OpCode.Code) {
            case Code.Nop:
                return null;
            case Code.Ldc_I4_M1:
                return this.Const(-1, this.typeSystem.Int32);
            case Code.Ldc_I4_0:
                return this.Const(0, this.typeSystem.Int32);
            case Code.Ldc_I4_1:
                return this.Const(1, this.typeSystem.Int32);
            case Code.Ldc_I4_2:
                return this.Const(2, this.typeSystem.Int32);
            case Code.Ldc_I4_3:
                return this.Const(3, this.typeSystem.Int32);
            case Code.Ldc_I4_4:
                return this.Const(4, this.typeSystem.Int32);
            case Code.Ldc_I4_5:
                return this.Const(5, this.typeSystem.Int32);
            case Code.Ldc_I4_6:
                return this.Const(6, this.typeSystem.Int32);
            case Code.Ldc_I4_7:
                return this.Const(7, this.typeSystem.Int32);
            case Code.Ldc_I4_8:
                return this.Const(8, this.typeSystem.Int32);
            case Code.Ldc_I4_S:
                return this.Const((int)(sbyte)inst.Operand, this.typeSystem.Int32);
            case Code.Ldc_I4:
                return this.Const((int)inst.Operand, this.typeSystem.Int32);
            case Code.Ldstr:
                return this.Const((string)inst.Operand, this.typeSystem.String);
            case Code.Ldarg_0:
                return this.LdArg(0);
            case Code.Ldarg_1:
                return this.LdArg(1);
            case Code.Ldarg_2:
                return this.LdArg(2);
            case Code.Starg_S:
                return this.StArg(((ParameterDefinition)inst.Operand).Index);
            case Code.Ldloc_0:
                return this.LdLoc(0);
            case Code.Ldloc_1:
                return this.LdLoc(1);
            case Code.Ldloc_2:
                return this.LdLoc(2);
            case Code.Ldloc_3:
                return this.LdLoc(3);
            case Code.Ldloc_S:
                return this.LdLoc(((VariableDefinition)inst.Operand).Index);
            case Code.Stloc_0:
                return this.StLoc(0);
            case Code.Stloc_1:
                return this.StLoc(1);
            case Code.Stloc_2:
                return this.StLoc(2);
            case Code.Stloc_3:
                return this.StLoc(3);
            case Code.Stloc_S:
                return this.StLoc(((VariableDefinition)inst.Operand).Index);
            case Code.Add:
                return this.SsaLocalAssignment(this.Binary(BinaryOp.Add));
            case Code.Sub:
                return this.SsaLocalAssignment(this.Binary(BinaryOp.Sub));
            case Code.Mul:
                return this.SsaLocalAssignment(this.Binary(BinaryOp.Mul));
            case Code.Div:
                return this.SsaLocalAssignment(this.Binary(BinaryOp.Div));
            case Code.Ceq:
                return this.SsaLocalAssignment(this.Binary(BinaryOp.Equal, this.typeSystem.Boolean));
            case Code.Clt:
                return this.SsaLocalAssignment(this.Binary(BinaryOp.LessThan, this.typeSystem.Boolean));
            case Code.Cgt:
                return this.SsaLocalAssignment(this.Binary(BinaryOp.GreaterThan, this.typeSystem.Boolean));
            case Code.Br_S:
            case Code.Br:
                return null;
            case Code.Brtrue_S:
            case Code.Brtrue:
                return this.SsaInstResultAssignment(inst, () => this.stack.Pop());
            case Code.Brfalse_S:
            case Code.Brfalse:
                return this.SsaInstResultAssignment(inst, () => this.Unary(UnaryOp.Not));
            case Code.Beq_S:
            case Code.Beq:
                return this.SsaInstResultAssignment(inst, () => this.Binary(BinaryOp.Equal));
            case Code.Bne_Un_S:
            case Code.Bne_Un:
                return this.SsaInstResultAssignment(inst, () => this.Binary(BinaryOp.NotEqual));
            case Code.Blt_S:
            case Code.Blt:
                return this.SsaInstResultAssignment(inst, () => this.Binary(BinaryOp.LessThan));
            case Code.Ble_S:
            case Code.Ble:
                return this.SsaInstResultAssignment(inst, () => this.Binary(BinaryOp.LessThanOrEqual));
            case Code.Bge_S:
            case Code.Bge:
                return this.SsaInstResultAssignment(inst, () => this.Binary(BinaryOp.GreaterThanOrEqual));
            case Code.Ret:
                return new StmtReturn(this.method.ReturnType.IsVoid() ? null : this.stack.Pop());
            case Code.Call:
                return this.Call(inst);
            default:
                throw new NotImplementedException("Cannot handle: " + inst.OpCode);
            }
        }

        private Stmt Call(Instruction inst) {
            var calling = (MethodReference)inst.Operand;
            var args = new List<Expr>();
            for (int i = 0; i < calling.Parameters.Count; i++) {
                var expr = this.stack.Pop();
                args.Add(expr);
            }
            args.Reverse();
            if (calling.ReturnType.IsVoid()) {
                return new StmtCall(calling, args);
            } else {
                return this.SsaLocalAssignment(new ExprCall(calling, args));
            }
        }

        private Stmt Const(object value, TypeReference type) {
            return this.SsaLocalAssignment(new ExprLiteral(value, type));
        }

        private Stmt LdArg(int idx) {
            return this.SsaLocalAssignment(() => {
                int argIdx;
                if (this.method.HasThis) {
                    if (idx == 0) {
                        return new ExprThis(this.method.DeclaringType);
                    } else {
                        argIdx = idx - 1;
                    }
                } else {
                    argIdx = idx;
                }
                return this.args[argIdx];
            });
        }

        private Stmt StArg(int idx) {
            var expr = this.stack.Pop();
            var target = new ExprVarLocal(expr.Type);
            var assignment = new StmtAssignment(target, expr);
            this.args[idx] = target;
            return assignment;
        }

        private Stmt LdLoc(int idx) {
            return this.SsaLocalAssignment(this.locals[idx]);
        }

        private Stmt StLoc(int idx) {
            var expr = this.stack.Pop();
            var target = new ExprVarLocal(expr.Type);
            var assignment = new StmtAssignment(target, expr);
            this.locals[idx] = target;
            return assignment;
        }

        private ExprUnary Unary(UnaryOp op, TypeReference type = null) {
            var e = this.stack.Pop();
            return new ExprUnary(op, type ?? e.Type, e);
        }

        private ExprBinary Binary(BinaryOp op, TypeReference type = null) {
            var right = this.stack.Pop();
            var left = this.stack.Pop();
            return new ExprBinary(op, type ?? left.Type, left, right);
        }

        private Stmt SsaLocalAssignment(Func<Expr> exprFn) {
            return this.SsaLocalAssignment(exprFn());
        }

        private Stmt SsaLocalAssignment(Expr expr) {
            var target = new ExprVarLocal(expr.Type);
            var assignment = new StmtAssignment(target, expr);
            this.stack.Push(target);
            return assignment;
        }

        private Stmt SsaInstResultAssignment(Instruction inst, Func<Expr> fnExpr) {
            var expr = fnExpr();
            var target = this.instResults[inst];
            var assignment = new StmtAssignment(target, expr);
            return assignment;
        }

    }
}
