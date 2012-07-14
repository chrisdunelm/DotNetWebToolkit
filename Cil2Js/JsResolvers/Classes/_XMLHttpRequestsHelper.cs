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

    class _XMLHttpRequestsHelper {

        [Js]
        public static Stmt Encode(Ctx ctx) {
            var tRef = ((GenericInstanceMethod)ctx.MRef).GenericArguments[0];
            var methods = tRef.EnumResolvedMethods().ToArray();
            var propertySetters = methods.Where(x => x.Name.StartsWith("set_")).ToArray();
            var arg = ctx.MethodParameter(0);
            var calls = propertySetters.Select((s, i) => {
                var fieldName = s.Name.Substring(4);
                var get = methods.First(x => x.Name == "get_" + fieldName);
                var call = new ExprCall(ctx, get, arg);
                var paramType = s.Parameters[0].ParameterType;
                if (paramType.IsNullable()) {
                    paramType = paramType.GetNullableInnerType();
                }
                Expr expr;
                switch (paramType.MetadataType) {
                case MetadataType.SByte:
                case MetadataType.Byte:
                case MetadataType.Int16:
                case MetadataType.UInt16:
                case MetadataType.Int32:
                case MetadataType.UInt32:
                case MetadataType.Int64:
                case MetadataType.UInt64:
                case MetadataType.Boolean:
                case MetadataType.String:
                    expr = call;
                    break;
                case MetadataType.Char:
                    expr = new ExprJsExplicit(ctx, "String.fromCharCode(call)", ctx.Char, call.Named("call"));
                    break;
                case MetadataType.Class:
                case MetadataType.ValueType:
                    var mNested = ctx.MRef.GetElementMethod().MakeGeneric(paramType);
                    expr = new ExprCall(ctx, mNested, null, call);
                    break;
                default:
                    throw new InvalidOperationException("Cannot handle: " + paramType.MetadataType);
                }
                return new { js = "'" + fieldName + "': call" + i, expr = expr.Named("call" + i) };
            }).ToArray();
            var js = "";
            var args = calls.Select(x => x.expr);
            if (!tRef.IsValueType) {
                js = "if (arg === null) return null;" + Environment.NewLine;
                args = args.Concat(arg.Named("arg"));
            }
            js += "return { " + string.Join(", ", calls.Select(x => x.js)) + " };";
            return new StmtJsExplicit(ctx, js, args);
        }

        [Js]
        public static Stmt Decode(Ctx ctx) {
            var tRef = ((GenericInstanceMethod)ctx.MRef).GenericArguments[0];
            var methods = tRef.EnumResolvedMethods().ToArray();
            var propertySetters = methods.Where(x => x.Name.StartsWith("set_")).ToArray();
            var obj = ctx.Local(tRef);
            var objInit = new StmtJsExplicit(ctx, "obj = { _: objType };", obj.Named("obj"), new ExprJsTypeVarName(ctx, tRef).Named("objType"));
            var arg = ctx.MethodParameter(0, "arg");
            var calls = propertySetters.Select(s => {
                var fieldName = s.Name.Substring(4);
                var paramType = s.Parameters[0].ParameterType;
                var rawValue = new ExprJsExplicit(ctx, "arg." + fieldName, paramType, arg);
                var rawValueNamed = rawValue.Named("rawValue");
                if (paramType.IsNullable()) {
                    paramType = paramType.GetNullableInnerType();
                }
                Expr value;
                switch (paramType.MetadataType) {
                case MetadataType.SByte:
                case MetadataType.Byte:
                case MetadataType.Int16:
                case MetadataType.UInt16:
                case MetadataType.Int32:
                case MetadataType.UInt32:
                case MetadataType.Int64:
                case MetadataType.UInt64:
                case MetadataType.Boolean:
                case MetadataType.String:
                    value = rawValue;
                    break;
                case MetadataType.Char:
                    value = new ExprJsExplicit(ctx, "rawValue ? rawValue.charCodeAt(0) : 0", ctx.Char, rawValueNamed);
                    break;
                case MetadataType.Class:
                case MetadataType.ValueType:
                    var mNested = ctx.MRef.GetElementMethod().MakeGeneric(paramType);
                    value = new ExprCall(ctx, mNested, null, rawValue);
                    if (!paramType.IsValueType) {
                        value = new ExprJsExplicit(ctx, "rawValue ? call : null", paramType, rawValueNamed, value.Named("call"));
                    }
                    break;
                default:
                    throw new InvalidOperationException("Cannot handle: " + paramType.MetadataType);
                }
                var call = new ExprCall(ctx, s, obj, value);
                return call;
            }).ToArray();
            var ret = new StmtReturn(ctx, obj);
            return new StmtBlock(ctx, calls.Select(x => (Stmt)new StmtWrapExpr(ctx, x)).Prepend(objInit).Concat(ret));
        }

    }

}
