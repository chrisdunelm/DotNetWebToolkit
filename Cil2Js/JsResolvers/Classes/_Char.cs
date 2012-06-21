using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    class _Char {

        [JsRedirect(typeof(char))]
        public override string ToString() {
            throw new JsImplException();
        }
        [Js]
        public static Stmt ToString(Ctx ctx) {
            var js = "return String.fromCharCode(this);";
            var stmt = new StmtJsExplicit(ctx, js, ctx.ThisNamed);
            return stmt;
        }

        [JsRedirect(typeof(char))]
        public override int GetHashCode() {
            return base.GetHashCode();
        }
        [Js]
        public static Stmt GetHashCode(Ctx ctx) {
            return new StmtJsExplicit(ctx, "return this | this << 16;", ctx.ThisNamed);
        }

        [JsRedirect(typeof(Char))]
        public override bool Equals(object obj) {
            throw new JsImplException();
        }
        [Js(typeof(bool), typeof(object))]
        public static Stmt Equals(Ctx ctx) {
            var other = ctx.MethodParameter(0).Named("other");
            var type = new ExprJsTypeVarName(ctx, ctx.Char).Named("type");
            return new StmtJsExplicit(ctx, "return other._ === type && this === other.v;", ctx.ThisNamed, other, type);
        }

        public static bool Equals([JsFakeThis]Char _this, Char other) {
            return _this == other;
        }

        public static int CompareTo([JsFakeThis]char _this, char other) {
            return (int)(_this - other);
        }

        public static int CompareTo([JsFakeThis]char _this, object other) {
            if (other == null) {
                return 1;
            }
            if (!(other is char)) {
                throw new ArgumentException();
            }
            return (int)(_this - (char)other);
        }

        [Js]
        public static Stmt IsWhiteSpace(Ctx ctx) {
            // See http://msdn.microsoft.com/en-us/library/t809ektx.aspx for list
            var whiteSpace = " \u1680\u180e\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200a\u202f\u205f\u3000\u2028\u2029\u0009\u000a\u000b\u000c\u000d\u0085\u00a0";
            var arg0 = ctx.MethodParameter(0);
            var c = new ExprJsResolvedMethod(ctx, ctx.String, null, "String.fromCharCode", arg0);
            var indexOf = new ExprJsResolvedMethod(ctx, ctx.Boolean, ctx.Literal(whiteSpace), "indexOf", c);
            return new StmtReturn(ctx,
                new ExprBinary(ctx, BinaryOp.GreaterThanOrEqual, ctx.Boolean, indexOf, ctx.Literal(0)));
        }

    }
}
