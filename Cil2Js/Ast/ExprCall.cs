﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;
using System.Reflection;

namespace DotNetWebToolkit.Cil2Js.Ast {

    public class ExprCall : Expr, ICall {

        public ExprCall(Ctx ctx, MethodReference callMethod, Expr obj, IEnumerable<Expr> args, bool isVirtualCall, TypeReference constrainedType, TypeReference forceReturnType)
            : base(ctx) {
            this.CallMethod = callMethod;
            this.Obj = obj;
            this.Args = args;
            this.IsVirtualCall = isVirtualCall;
            this.ConstrainedType = constrainedType;
            this.returnType = forceReturnType ?? callMethod.ReturnType.FullResolve(callMethod);
        }

        public ExprCall(Ctx ctx, MethodReference callMethod, Expr obj, IEnumerable<Expr> args, bool isVirtualCall, TypeReference constrainedType)
            :this(ctx, callMethod, obj, args, isVirtualCall, constrainedType, null){
        }

        public ExprCall(Ctx ctx, MethodReference callMethod, Expr obj, IEnumerable<Expr> args, bool isVirtualCall)
            : this(ctx, callMethod, obj, args, isVirtualCall, null) {
        }

        public ExprCall(Ctx ctx, MethodReference callMethod, Expr obj, params Expr[] args)
            : this(ctx, callMethod, obj, (IEnumerable<Expr>)args, false) {
        }

        public ExprCall(Ctx ctx, TypeReference forceReturnType, MethodReference callMethod, Expr obj, params Expr[] args)
            : this(ctx, callMethod, obj, (IEnumerable<Expr>)args, false, null, forceReturnType) {
        }

        public ExprCall(Ctx ctx, MethodInfo callMethod, Expr obj, params Expr[] args)
            : this(ctx, ctx.Module.Import(callMethod), obj, (IEnumerable<Expr>)args, callMethod.IsVirtual) {
        }

        public ExprCall(Ctx ctx, Delegate callMethod, Expr obj, params Expr[] args)
            : this(ctx, ctx.Module.Import(callMethod.Method), obj, (IEnumerable<Expr>)args, callMethod.Method.IsVirtual) {
        }

        public Expr Obj { get; private set; }
        public MethodReference CallMethod { get; private set; }
        public IEnumerable<Expr> Args { get; private set; }
        public bool IsVirtualCall { get; private set; }
        public TypeReference ConstrainedType { get; private set; }

        private TypeReference returnType;

        public bool IsStatic {
            get { return this.Obj == null; }
        }

        public override Expr.NodeType ExprType {
            get { return NodeType.Call; }
        }

        public override TypeReference Type {
            get { return this.returnType; }
        }

        public override Special Specials {
            get { return Special.PossibleSideEffects; }
        }

        public override string ToString() {
            return string.Format("{0}.{1}({2})", this.IsStatic ? (object)this.CallMethod.DeclaringType : this.Obj, this.CallMethod.Name,
                string.Join(", ", this.Args));
        }

    }
}
