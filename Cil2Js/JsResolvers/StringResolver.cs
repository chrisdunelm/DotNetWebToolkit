using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Cil2Js.Output;
using Mono.Cecil;

namespace Cil2Js.JsResolvers {
    static class StringResolver {

        public static JsResolved op_Equality(Expr.Gen exprGen, ICall method) {
            var left = method.Args.ElementAt(0);
            var right = method.Args.ElementAt(1);
            var expr = exprGen.Equal(left, right);
            return new JsResolvedExpr(expr);
        }

        public static JsResolved get_Length(Expr.Gen exprGen, ICall method) {
            return new JsResolvedProperty(method.Obj, "length");
        }

        public static JsResolved get_Chars(Expr.Gen exprGen, ICall method) {
            return new JsResolvedMethod(method.Obj, "charAt", method.Args.First());
        }

        public static JsResolved ConcatStrings(Expr.Gen exprGen, ICall method) {
            var expr = method.Args.Aggregate((a, b) => exprGen.Add(a, b));
            return new JsResolvedExpr(expr);
        }

        public static JsResolved ConcatStringsMany(Expr.Gen exprGen, ICall method) {
            return new JsResolvedMethod(method.Args.First(), "join", new ExprLiteral("", exprGen.TypeSystem.String));
        }

        public static JsResolved IndexOf(Expr.Gen exprGen, ICall method) {
            return new JsResolvedMethod(method.Obj, "indexOf", method.Args);
        }

        public static JsResolved Substring(Expr.Gen exprGen, ICall method) {
            return new JsResolvedMethod(method.Obj, "substr", method.Args);
        }

    }
}
