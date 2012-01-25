using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class ExprJsVirtualCall : Expr, ICall {

        //public ExprJsVirtualCall(Ctx ctx, MethodReference callMethod, Expr objInit, Expr objRef, IEnumerable<Expr> args)
        public ExprJsVirtualCall(Ctx ctx, MethodReference callMethod, Expr runtimeType, Expr objRef, IEnumerable<Expr> args)
            : base(ctx) {
            this.CallMethod = callMethod;
            //this.ObjInit = objInit;
            this.RuntimeType = runtimeType;
            this.ObjRef = objRef;
            this.Args = args;
            this.returnType = callMethod.ReturnType.FullResolve(callMethod);
        }

        public MethodReference CallMethod { get; private set; }
        //public Expr ObjInit { get; private set; }
        public Expr RuntimeType { get; private set; }
        public Expr ObjRef { get; private set; }
        public IEnumerable<Expr> Args { get; private set; }

        private TypeReference returnType;

        public override Expr.NodeType ExprType {
            get { return (Expr.NodeType)JsExprType.JsVirtualCall; }
        }

        public override TypeReference Type {
            get { return this.returnType; }
        }

        public override Special Specials {
            get { return Special.PossibleSideEffects; }
        }

        public bool IsVirtualCall {
            get { return true; }
        }

        public Expr Obj {
            get { return this.ObjRef; }
        }

    }
}
