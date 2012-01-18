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
        [Js(typeof(CreateArrayImpl))]
        public static T[] CreateArray<T>(int size) {
            throw new Exception();
        }

        class CreateArrayImpl : IJsImpl {
            public Stmt GetImpl(Ctx ctx) {
                var count = new ExprVarParameter(ctx, ctx.MRef.Parameters[0]);
                var elType = ((GenericInstanceMethod)ctx.MRef).GenericArguments[0];
                var arrayType = elType.MakeArray();
                var elTypeExpr = new ExprJsTypeVarName(ctx, arrayType);
                var defaultValue = new ExprDefaultValue(ctx, elType);
                var a = new ExprVarLocal(ctx, arrayType);
                var i = new ExprVarLocal(ctx, ctx.Int32);
                var js = "var {3}=new Array({0}); {3}._={1}; for(var {4}={0}-1;{4}>=0;{4}--) {3}[{4}]={2}; return {3};";
                var stmt = new StmtJsExplicitFunction(ctx, js, count, elTypeExpr, defaultValue, a, i);
                return stmt;
            }
        }

        public static object Cast(object o, Type type) {
            throw new Exception();
        }

        //public static bool IsAssignableTo(Type from, Type to) {
        //    // Rules from ECMA-335 partition III page 21
        //    // Rule 7
        //    if (from.IsArray && to.IsArray) {
        //        return IsAssignableTo(from.GetElementType(), to.GetElementType());
        //    }
        //    // Rules 1, 3 (incomplete, not interfaces) and 4
        //    var t = from;
        //    do {
        //        if (t == to) {
        //            return true;
        //        }
        //        t = t.BaseType;
        //    } while (t != null);
        //    if (to.IsInterface) {
        //        var interfaces = from.GetInterfaces();
        //        for (int i = 0; i < interfaces.Length; i++) {
        //            if (interfaces[i] == to) {
        //                return true;
        //            }
        //        }
        //    }
        //    // TODO: More rules
        //    return false;
        //}

    }
}
