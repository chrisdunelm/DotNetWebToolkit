using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    class _String : IEnumerable<char>, IEnumerable {

        public _String(char c, int repeatCount) {

        }

        [Js(".ctor", typeof(void), typeof(char), typeof(int))]
        public static Stmt ctorCharRepeatCount(Ctx ctx) {
            // TODO: This works, but the JS ctor has an extra 'return' statement at the end, which is never executed
            var @char = ctx.MethodParameter(0, "char");
            var count = ctx.MethodParameter(1, "count");
            return new StmtJsExplicit(ctx, "return Array(count + 1).join(String.fromCharCode(char));", @char, count);
        }

        [Js(typeof(bool), typeof(object))]
        [Js(typeof(bool), typeof(string))]
        public static Stmt Equals(Ctx ctx) {
            var other = ctx.MethodParameter(0, "other");
            var stmt = new StmtJsExplicit(ctx, "return this === other;", ctx.ThisNamed, other);
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
            var js = @"
acc = 5381;
for (i = Math.min(this.length - 1,100); i >= 0; i--)
    acc = ((acc << 5) + acc + this.charCodeAt(i)) & mask;
return acc;";
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

        public static bool StartsWith([JsFakeThis]string _this, string value) {
            return _this.Substring(0, value.Length) == value;
        }

        public static bool EndsWith([JsFakeThis]string _this, string value) {
            if (value.Length > _this.Length) {
                return false;
            }
            return _this.Substring(_this.Length - value.Length, value.Length) == value;
        }

        public static string[] Split([JsFakeThis]string _this, params char[] separator) {
            return _this.Split(separator, int.MaxValue);
        }

        [Js("Split", typeof(string[]), typeof(char[]), typeof(int))]
        public static Stmt Split(Ctx ctx) {
            var separators = ctx.MethodParameter(0, "separators");
            var limit = ctx.MethodParameter(1, "limit");
            var regex = ctx.Local(ctx.String, "regex");
            var i = ctx.Local(ctx.Int32, "i");
            var c = ctx.Local(ctx.String, "c");
            var js = @"
if (!separators || separators.length == 0) {
    regex = ' |\u1680|\u180e|[\u2000-\u200a]|\u202f|\u205f|\u3000|\u2028|\u2029|[\x09-\x0d]|\x85|\xa0';
} else {
    regex = '';
    for (i = 0; i < separators.length; i++) {
        if (i > 0) {
            regex += '|';
        }
        c = String.fromCharCode(separators[i]);
        if ('[().^$|?*+\\'.indexOf(c) >= 0) {
            c = '\\' + c;
        }
        regex += c;
    }
}
return this.split(new RegExp(regex, ''), limit);
";
            return new StmtJsExplicit(ctx, js, ctx.ThisNamed, separators, limit, regex, i, c);
        }

        [Js("Join", typeof(string), typeof(string), typeof(string[]))]
        public static Expr Join(ICall call) {
            var ctx = call.Ctx;
            var separator = call.Arg(0, "separator");
            var values = call.Arg(1);
            var sepExpr = new ExprJsExplicit(ctx, "separator || \"\"", ctx.String, separator);
            var expr = new ExprJsResolvedMethod(ctx, ctx.String, values, "join", sepExpr);
            return expr;
        }

        public static string Join(string separator, object[] values) {
            var s = new string[values.Length];
            for (int i = 0; i < values.Length; i++) {
                var o = values[i];
                s[i] = o == null ? null : o.ToString();
            }
            return string.Join(separator, s);
        }

        public static string Join(string separator, IEnumerable<string> values) {
            return string.Join(separator, values.ToArray());
        }

        public static string Join<T>(string separator, IEnumerable<T> values) {
            return string.Join(separator, values.Select(x => x.ToString()).ToArray());
        }

        public static string Format(string format, object arg0) {
            return string.Format(format, new[] { arg0 });
        }

        public static string Format(string format, object arg0, object arg1) {
            return string.Format(format, new[] { arg0, arg1 });
        }

        public static string Format(string format, object arg0, object arg1, object arg2) {
            return string.Format(format, new[] { arg0, arg1, arg2 });
        }

        public static string Format(string format, object[] args) {
            var result = new StringBuilder(format.Length);
            var fmtLength = format.Length;
            bool inFmtItem = false, inCloseBrace = false;
            var fmtItem = new StringBuilder(20);
            for (int i = 0; i < fmtLength; i++) {
                var c = format[i];
                if (inCloseBrace) {
                    if (c != '}') {
                        throw new FormatException();
                    }
                    result.Append('}');
                    inCloseBrace = false;
                } else if (inFmtItem) {
                    if (fmtItem.Length == 0 && c == '{') {
                        result.Append('{');
                        inFmtItem = false;
                    } else {
                        if (c == '{') {
                            throw new FormatException();
                        }
                        if (c == '}') {
                            // FmtItem complete - process it
                            var fmtItemStr = fmtItem.ToString();
                            var fmtItemLen = fmtItemStr.Length;
                            int commaPos = fmtItemStr.IndexOf(',');
                            int colonPos = fmtItemStr.IndexOf(':');
                            string indexStr, alignStr, fmtStr;
                            indexStr = fmtItemStr.Substring(0, commaPos >= 0 ? commaPos : (colonPos >= 0 ? colonPos : fmtItemLen));
                            alignStr = commaPos == -1 ? null : fmtItemStr.Substring(commaPos + 1, (colonPos >= 0 ? colonPos : fmtItemLen) - commaPos - 1);
                            fmtStr = colonPos == -1 ? null : fmtItemStr.Substring(colonPos + 1, fmtItemLen - colonPos - 1);
                            int index = int.Parse(indexStr);
                            if (index < 0 || index > args.Length) {
                                throw new FormatException();
                            }
                            string s;
                            var arg = args[index];
                            if (arg == null) {
                                s = "";
                            } else {
                                var argFormattable = arg as IFormattable;
                                if (argFormattable != null) {
                                    s = argFormattable.ToString(fmtStr, null);
                                } else {
                                    s = arg.ToString();
                                }
                            }
                            if (alignStr != null) {
                                var align = int.Parse(alignStr);
                                var padLen = Math.Abs(align) - s.Length;
                                if (padLen > 0) {
                                    var padStr = new string(' ', padLen);
                                    s = align >= 0 ? padStr + s : s + padStr;
                                }
                            }
                            result.Append(s);
                            // Continue with format-string parsing
                            fmtItem.Length = 0;
                            inFmtItem = false;
                        } else {
                            fmtItem.Append(c);
                        }
                    }
                } else {
                    if (c == '{') {
                        inFmtItem = true;
                    } else if (c == '}') {
                        inCloseBrace = true;
                    } else {
                        result.Append(c);
                    }
                }
            }
            return result.ToString();
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

        [Js(typeof(int), typeof(string))]
        public static Stmt CompareTo(Ctx ctx) {
            return new StmtJsExplicit(ctx, "return this === a ? 0 : (this < a ? -1 : 1);", ctx.ThisNamed, ctx.MethodParameter(0, "a"));
        }

        public static int CompareTo([JsFakeThis]string _this, object other) {
            if (other == null) {
                return 1;
            }
            if (!(other is string)) {
                throw new ArgumentException();
            }
            return _this.CompareTo((string)other);
        }

        [Js]
        public static Expr IsNullOrEmpty(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsExplicit(ctx, "(!a)", ctx.Boolean, ctx.MethodParameter(0, "a"));
        }

        public static bool IsNullOrWhiteSpace(string s) {
            return s == null || s.All(char.IsWhiteSpace);
        }

        [Js("Replace", typeof(string), typeof(char), typeof(char))]
        public static Expr ReplaceChar(ICall call) {
            var ctx = call.Ctx;
            var args = call.Args.Select(x => new ExprJsResolvedMethod(ctx, ctx.Char, null, "String.fromCharCode", x)).ToArray();
            return new ExprJsResolvedMethod(ctx, ctx.String, call.Obj, "replace", args);
        }

        public static string Replace([JsFakeThis]string _this, string oldValue, string newValue) {
            if (oldValue == null) {
                throw new ArgumentNullException();
            }
            if (oldValue == "") {
                throw new ArgumentException();
            }
            return InternalReplaceString(_this, oldValue, newValue);
        }

        [JsRedirect]
        private static string InternalReplaceString(string s, string oldValue, string newValue) {
            throw new JsImplException();
        }
        [Js]
        public static Expr InternalReplaceString(ICall call) {
            var ctx = call.Ctx;
            var args = new[] {
                call.Arg(1),
                new ExprJsResolvedMethod(ctx, ctx.String, call.Arg(2), "replace", ctx.Literal("$"), ctx.Literal("$$$$"))
            };
            return new ExprJsResolvedMethod(ctx, ctx.String, call.Arg(0), "replace", args);
        }

    }
}
