using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {
    class InternalFunctions {

        // TODO: This way of creating/initialising arrays is not great.
        // A new method will be created for each type of array
        // Although the other option requires getting default value at runtime, also not great
        [Js(typeof(CreateArrayImpl))]
        public static T[] CreateArray<T>(int size) {
            throw new Exception();
        }

        class CreateArrayImpl : IJsImpl {
            public Stmt GetImpl(Ctx ctx) {
                var count = ctx.MethodParameter(0);
                var elType = ((GenericInstanceMethod)ctx.MRef).GenericArguments[0];
                var arrayType = elType.MakeArray();
                var elTypeExpr = new ExprJsTypeVarName(ctx, arrayType);
                var defaultValue = new ExprDefaultValue(ctx, elType);
                var a = new ExprVarLocal(ctx, arrayType);
                var i = new ExprVarLocal(ctx, ctx.Int32);
                var js = "{3}=new Array({0}); {3}._={1}; for({4}={0}-1;{4}>=0;{4}--) {3}[{4}]={2}; return {3};";
                var stmt = new StmtJsExplicitFunction(ctx, js, count, elTypeExpr, defaultValue, a, i);
                return stmt;
            }
        }

        [Js(typeof(CanAssignToImpl))]
        private static bool CanAssignTo(object o, Type toType) {
            throw new Exception();
        }

        class CanAssignToImpl : IJsImpl {
            public Stmt GetImpl(Ctx ctx) {
                var obj = ctx.MethodParameter(0);
                var toType = ctx.MethodParameter(1);
                var canCastTo = new ExprVarLocal(ctx, ctx.Type.MakeArray());
                var mGetType = ctx.Module.Import(typeof(object).GetMethod("GetType"));
                var getTypeCall = new ExprCall(ctx, mGetType, obj);
                var assignableTo = new ExprJsTypeData(ctx, TypeData.AssignableTo);
                var i = new ExprVarLocal(ctx, ctx.Int32);
                var t = new ExprVarLocal(ctx, ctx.Type);
                var js = "if (!{0}) return true; {6}={3}; if ({6}==={1}) return true; {2}={6}.{4}; for ({5}={2}.length-1;{5}>=0;{5}--) if ({2}[{5}]==={1}) return true; return false;";
                var stmt = new StmtJsExplicitFunction(ctx, js, obj, toType, canCastTo, getTypeCall, assignableTo, i, t);
                return stmt;
            }
        }


        [Js(typeof(IsInstImpl))]
        public static object IsInst(object o, Type toType) {
            throw new Exception();
        }

        class IsInstImpl : IJsImpl {
            public Stmt GetImpl(Ctx ctx) {
                var obj = ctx.MethodParameter(0);
                var toType = ctx.MethodParameter(1);
                var callCanAssignTo = new ExprCall(ctx, (Func<object, Type, bool>)CanAssignTo, null, obj, toType);
                var js = "return {0}?{1}:null;";
                var stmt = new StmtJsExplicitFunction(ctx, js, callCanAssignTo, obj);
                return stmt;
            }
        }


        [Js(typeof(CastImpl))]
        public static object Cast(object o, Type toType) {
            throw new Exception();
        }

        class CastImpl : IJsImpl {
            public Stmt GetImpl(Ctx ctx) {
                var obj = ctx.MethodParameter(0);
                var toType = ctx.MethodParameter(1);
                var callCanAssignTo = new ExprCall(ctx, (Func<object, Type, bool>)CanAssignTo, null, obj, toType);
                var mCtorEx = ctx.Module.Import(typeof(InvalidCastException).GetConstructor(new[] { typeof(string) }));
                var mGetType = ctx.Module.Import(typeof(object).GetMethod("GetType"));
                var getTypeCall = new ExprCall(ctx, mGetType, obj);
                // "Unable to cast object of type 'System.String' to type 'Test.ExecutionTests.TestCasting'."
                var msgParts = new Expr[] {
                    new ExprLiteral(ctx, "Unable to cast object of type '", ctx.String),
                    new ExprCall(ctx, typeof(Type).GetProperty("FullName").GetMethod, getTypeCall),
                    new ExprLiteral(ctx, "' to type '", ctx.String),
                    new ExprCall(ctx, typeof(Type).GetProperty("FullName").GetMethod, toType),
                    new ExprLiteral(ctx, "'.", ctx.String)
                };
                var msg = msgParts.Aggregate((a, b) => new ExprBinary(ctx, BinaryOp.Add, ctx.String, a, b));
                var ctorEx = new ExprNewObj(ctx, mCtorEx, msg);
                var js = "if ({0}) return {1}; throw {2}";
                var stmt = new StmtJsExplicitFunction(ctx, js, callCanAssignTo, obj, ctorEx);
                return stmt;
            }
        }

    }
}
