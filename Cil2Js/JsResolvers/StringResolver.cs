using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil;

namespace Cil2Js.JsResolvers {
    static class StringResolver {

        public static Expr op_Equality(Expr.Gen exprGen, ICall method) {
            var left = method.Args.ElementAt(0);
            var right = method.Args.ElementAt(1);
            return exprGen.Equal(left, right);
        }

    }
}
