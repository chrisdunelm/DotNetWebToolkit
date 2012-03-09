using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    //[Js("ToString", typeof(string))]
    //[Js("Concat", typeof(string), typeof(object), typeof(object))]
    //[Js("Concat", typeof(string), typeof(object), typeof(object), typeof(object))]
    class _String : IEnumerable<char>, IEnumerable {

        [Js(typeof(bool), typeof(object))]
        [Js(typeof(bool), typeof(string))]
        public static Stmt Equals(Ctx ctx) {
            var other = ctx.MethodParameter(0, "other");
            var stmt = new StmtJsExplicit(ctx, "return this===other;", ctx.ThisNamed, other);
            return stmt;
        }

        [JsRedirect(typeof(string))]
        public override int GetHashCode() {
            throw new JsImplException();
        }
        [Js]
        public static Stmt GetHashCode(Ctx ctx) {
            var acc = new ExprVarLocal(ctx, ctx.Int32).Named("acc");
            var i = new ExprVarLocal(ctx, ctx.Int32).Named("i");
            var mask = new ExprLiteral(ctx, 0x7fffffff, ctx.Int32).Named("mask");
            var js = "acc=5381;for(i=Math.min(this.length-1,100);i>=0;i--) acc=((acc<<5)+acc+this.charCodeAt(i))&mask; return acc;";
            var stmt = new StmtJsExplicit(ctx, js, ctx.ThisNamed, acc, i, mask);
            return stmt;
        }

        [JsRedirect(typeof(string))]
        public override string ToString() {
            throw new JsImplException();
        }
        [Js]
        public static Stmt ToString(Ctx ctx) {
            return new StmtJsExplicit(ctx, "return this;", ctx.ThisNamed);
        }

        [Js(typeof(int))]
        public static Expr get_Length(ICall call) {
            return new ExprJsResolvedProperty(call.Ctx, call, "length");
        }

        [Js(typeof(char), typeof(int))]
        public static Expr get_Chars(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.Char, call.Obj, "charCodeAt", call.Args.First());
        }

        [Js(typeof(string), typeof(string), typeof(string))]
        [Js(typeof(string), typeof(string), typeof(string), typeof(string))]
        [Js(typeof(string), typeof(string), typeof(string), typeof(string), typeof(string))]
        public static Expr Concat(ICall call) {
            var expr = call.Args.Aggregate((a, b) => call.Ctx.ExprGen.Add(a, b));
            return expr;
        }

        [Js("Concat", typeof(string), typeof(string[]))]
        public static Expr ConcatStringsArray(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.String, call.Args.First(), "join", new ExprLiteral(ctx, "", ctx.String));
        }

        public static string Concat(object arg0) {
            return string.Concat(new[] { arg0 });
        }

        public static string Concat(object arg0, object arg1) {
            return string.Concat(new[] { arg0, arg1 });
        }

        public static string Concat(object arg0, object arg1, object arg2) {
            return string.Concat(new[] { arg0, arg1, arg2 });
        }

        public static string Concat(params object[] args) {
            string ret = "";
            foreach (var arg in args) {
                ret += (arg ?? "").ToString();
            }
            return ret;
        }

        [Js("IndexOf", typeof(int), typeof(char))]
        [Js("IndexOf", typeof(int), typeof(char), typeof(int))]
        public static Expr IndexOfChar(ICall call) {
            var ctx = call.Ctx;
            var args = call.Args.Select((x, i) => i != 0 ? x : new ExprJsResolvedMethod(ctx, ctx.Char, null, "String.fromCharCode", x));
            var expr = new ExprJsResolvedMethod(ctx, ctx.Int32, call.Obj, "indexOf", args);
            return expr;
        }

        [Js("IndexOf", typeof(int), typeof(string))]
        [Js("IndexOf", typeof(int), typeof(string), typeof(int))]
        public static Expr IndexOfString(ICall call) {
            var ctx = call.Ctx;
            var expr = new ExprJsResolvedMethod(ctx, ctx.Int32, call.Obj, "indexOf", call.Args);
            return expr;
        }

        [Js]
        public static Expr Substring(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.String, call.Obj, "substr", call.Args);
        }

        [Js]
        public static Expr op_Equality(ICall call) {
            var left = call.Args.ElementAt(0);
            var right = call.Args.ElementAt(1);
            var expr = call.Ctx.ExprGen.Equal(left, right);
            return expr;
        }

        [Js]
        public static Expr op_Inequality(ICall call) {
            var left = call.Args.ElementAt(0);
            var right = call.Args.ElementAt(1);
            var expr = call.Ctx.ExprGen.NotEqual(left, right);
            return expr;
        }

        class CharEnum : IEnumerator<char>, IEnumerator {

            public CharEnum(string s) {
                this.s = s;
                this.i = -1;
            }

            private string s;
            private int i;

            public char Current {
                get { return this.s[this.i]; }
            }

            public void Dispose() {
            }

            object System.Collections.IEnumerator.Current {
                get { return this.s[this.i]; }
            }

            public bool MoveNext() {
                this.i++;
                return this.i < this.s.Length;
            }

            public void Reset() {
                this.i = -1;
            }
        }

        IEnumerator<char> IEnumerable<char>.GetEnumerator() {
            return new CharEnum((string)(object)this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new CharEnum((string)(object)this);
        }

    }
}
