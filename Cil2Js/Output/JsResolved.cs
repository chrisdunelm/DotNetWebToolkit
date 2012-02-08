using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Output {

    public class ExprJsResolvedMethod : Expr {

        public ExprJsResolvedMethod(Ctx ctx, TypeReference returnType, Expr obj, string methodName, params Expr[] args)
            : this(ctx, returnType, obj, methodName, (IEnumerable<Expr>)args) {
        }

        public ExprJsResolvedMethod(Ctx ctx, TypeReference returnType, Expr obj, string methodName, IEnumerable<Expr> args)
            : base(ctx) {
            this.returnType = returnType;
            this.Obj = obj;
            this.MethodName = methodName;
            this.Args = args;
        }

        public Expr Obj { get; private set; }
        public string MethodName { get; private set; }
        public IEnumerable<Expr> Args { get; private set; }
        private TypeReference returnType;

        public override Expr.NodeType ExprType {
            get { return (Expr.NodeType)JsExprType.JsResolvedMethod; }
        }

        public override TypeReference Type {
            get { return this.returnType; }
        }
    }

    public class ExprJsResolvedProperty : Expr {

        public ExprJsResolvedProperty(Ctx ctx, TypeReference returnType, Expr obj, string propertyName)
            : base(ctx) {
            this.Obj = obj;
            this.PropertyName = propertyName;
            this.returnType = returnType;
        }

        public Expr Obj { get; private set; }
        public string PropertyName { get; private set; }
        private TypeReference returnType;

        public override Expr.NodeType ExprType {
            get { return (Expr.NodeType)JsExprType.JsResolvedProperty; }
        }

        public override TypeReference Type {
            get { return this.returnType; }
        }
    }

    public class ExprJsResolvedCtor : Expr {

        public ExprJsResolvedCtor(Ctx ctx, string typeName, TypeReference type, IEnumerable<Expr> args)
            : base(ctx) {
            this.TypeName = typeName;
            this.type = type;
            this.Args = args;
        }

        public string TypeName { get; private set; }
        public IEnumerable<Expr> Args { get; private set; }
        private TypeReference type;

        public override Expr.NodeType ExprType {
            get { return (Expr.NodeType)JsExprType.JsResolvedCtor; }
        }

        public override TypeReference Type {
            get { return this.type; }
        }

    }

}
