using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Attributes;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Output {
    class VisitorJsResolveSpecialTypes : JsAstVisitor {

        class FakeCall : ICall {

            public FakeCall(ICall call) {
                this.call = call;
            }

            private ICall call;

            public Expr.NodeType ExprType { get { return this.call.ExprType; } }
            public MethodReference CallMethod { get { return null; } }
            public bool IsVirtualCall { get { return this.call.IsVirtualCall; } }
            public Expr Obj { get { return this.call.Obj; } }
            public IEnumerable<Expr> Args { get { return this.call.Args; } }
            public TypeReference Type { get { return this.call.Type; } }
            public CodeType CodeType { get { return this.call.CodeType; } }
            public Ctx Ctx { get { return this.call.Ctx; } }

            public object Clone() {
                throw new NotImplementedException();
            }

        }


        public static ICode V(ICode ast) {
            var v = new VisitorJsResolveSpecialTypes();
            return v.Visit(ast);
        }

        protected override Ast.ICode VisitJsResolvedProperty(ExprJsResolvedProperty e) {
            if (e.Call.CallMethod != null) {
                var ctx = e.Ctx;
                var mDef = e.Call.CallMethod.Resolve();
                if (mDef.IsSetter) {
                    var tRef = e.Call.CallMethod.Parameters.Last().ParameterType;
                    var tDef = tRef.Resolve();
                    var attr = tDef.GetCustomAttribute<JsStringEnumAttribute>();
                    if (attr != null) {
                        var prop = new ExprJsResolvedProperty(ctx, new FakeCall(e.Call), e.PropertyName).Named("prop");
                        var type = new ExprJsTypeVarName(ctx, tRef).Named("type");
                        var enumStringMap = new ExprJsTypeData(ctx, TypeData.EnumStringMap).Named("enumStringMap");
                        var value = e.Call.Args.Last().Named("value");
                        var js =  "prop = type.enumStringMap[value]";
                        var expr = new ExprJsExplicit(ctx, js, e.Type, type, enumStringMap, value, prop);
                        return expr;
                    }
                } else {
                    var tDef = e.Type.Resolve();
                    var attr = tDef.GetCustomAttribute<JsStringEnumAttribute>();
                    if (attr != null) {
                        var prop = new ExprJsResolvedProperty(ctx, new FakeCall(e.Call), e.PropertyName).Named("prop");
                        var type = new ExprJsTypeVarName(ctx, e.Type).Named("type");
                        var enumStringMap = new ExprJsTypeData(ctx, TypeData.EnumStringMap).Named("enumStringMap");
                        var js = "type.enumStringMap[prop]";
                        var expr = new ExprJsExplicit(ctx, js, e.Type, prop, type, enumStringMap);
                        return expr;
                    }
                }
            }
            return base.VisitJsResolvedProperty(e);
        }

    }
}
