using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    class _Json {

        [Js]
        public static Expr Parse(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsExplicit(ctx, "window.JSON.parse(arg)", ctx.Object, call.Arg(0, "arg"));
        }

        [Js]
        public static Stmt DecodeObj(Ctx ctx) {
            var tRef = ((GenericInstanceMethod)ctx.MRef).GenericArguments[0];
            var methods = tRef.EnumResolvedMethods().ToArray();
            var propertySetters = methods.Where(x => x.Name.StartsWith("set_")).ToArray();
            var obj = ctx.Local(tRef);
            var objInit = new StmtJsExplicit(ctx, "obj = { _: objType };", obj.Named("obj"), new ExprJsTypeVarName(ctx, tRef).Named("objType"));
            var calls = new List<Expr>();
            var arg = ctx.MethodParameter(0, "arg");
            foreach (var s in propertySetters) {
                var fieldName = s.Name.Substring(4);
                var paramType = s.Parameters[0].ParameterType;
                Expr value = new ExprJsExplicit(ctx, "arg." + fieldName, paramType, arg);
                if (paramType.IsChar()) {
                    value = new ExprJsExplicit(ctx, "(value ? value.charCodeAt(0) : 0)", ctx.Char, value.Named("value"));
                } else if (!paramType.IsPrimitive && !paramType.IsString() && !(paramType.IsNullable() && paramType.GetNullableInnerType().IsPrimitive)) {
                    var mNested = ctx.MRef.GetElementMethod().MakeGeneric(paramType);
                    var nestedCall = new ExprCall(ctx, mNested, null, value);
                    //value = nestedCall;
                    value = new ExprJsExplicit(ctx, "value ? nestedCall : null", paramType, value.Named("value"), nestedCall.Named("nestedCall"));
                }
                var call = new ExprCall(ctx, s, obj, value);
                calls.Add(call);
            }
            var ret = new StmtReturn(ctx, obj);
            return new StmtBlock(ctx, calls.Select(x => (Stmt)new StmtWrapExpr(ctx, x)).Prepend(objInit).Concat(ret));
        }

    }

}
