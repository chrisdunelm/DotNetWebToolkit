using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Attributes;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes.Helpers {
    sealed class Number {

        [JsRedirect]
        public static string FormatInt32(Int32 value, string format, NumberFormatInfo info) {
            throw new JsImplException();
        }
        [Js]
        public static Stmt FormatInt32(Ctx ctx) {
            var value = ctx.MethodParameter(0, "value");
            var format = ctx.MethodParameter(1, "format");
            var newFmtEx = new ExprNewObj(ctx, ctx.Module.Import(typeof(FormatException).GetConstructor(Type.EmptyTypes))).Named("newFmtEx");
            var valuePos = ctx.Local(ctx.Int32, "valuePos");
            var precision = ctx.Local(ctx.Int32.MakeNullable(), "precision");
            var fmt0 = ctx.Local(ctx.String, "fmt0");
            var s = ctx.Local(ctx.String, "s");
            var parts = ctx.Local(ctx.String.MakeArray(), "parts");
            var js = @"
format = format || 'G';
valuePos = Math.abs(value);
precision = format.length > 1 ? +format.substr(1) : null;
fmt0 = format.charAt(0);
switch (fmt0) {
case 'd':
case 'D':
    s = valuePos.toString();
    if (precision !== null && precision > s.length) {
        s = Array(precision - s.length + 1).join('0') + s;
    }
    break;
case 'g':
case 'G':
    s = valuePos.toString();
    if (precision !== null && precision < s.length && precision > 0) {
        s = valuePos.toPrecision(precision);
        parts = s.split('e');
        parts[0] = parts[0].replace(/(\d)\.?0+$/, '$1');
        parts[1] = parts[1].replace(/^\+(\d)$/, '+0$1');
        s = parts.join(fmt0 === 'g' ? 'e' : 'E');
    }
    break;
case 'n':
case 'N':
    if (precision === null) precision = 2;
    s = valuePos.toFixed(precision);
    parts = s.split('.');
    parts[0] = parts[0].replace(/(\d)(?=(\d{3})+$)/g, '$1,');
    s = parts.join('.');
    break;
case 'x':
case 'X':
    if (value < 0) {
        value += 0x100000000;
    }
    s = value.toString(16);
    if (fmt0 === 'X') {
        s = s.toUpperCase();
    }
    if (precision !== null && precision > s.length) {
        s = Array(precision - s.length + 1).join('0') + s;
    }
    break;
default:
    throw newFmtEx;
}
return value < 0 ? '-' + s : s;
";
            return new StmtJsExplicit(ctx, js, value, format, newFmtEx, valuePos, precision, fmt0, s, parts);
        }

        [JsRedirect]
        public static string FormatInt64(Int64 value, string format, NumberFormatInfo info) {
            throw new JsImplException();
        }
        [Js]
        public static Stmt FormatInt64(Ctx ctx) {
                        var value = ctx.MethodParameter(0, "value");
            var format = ctx.MethodParameter(1, "format");
            var neg = ctx.Local(ctx.Boolean, "neg");
            var precision = ctx.Local(ctx.Int32.MakeNullable(), "precision");
            var fmt0 = ctx.Local(ctx.String, "fmt0");
            var i = ctx.Local(ctx.Int32, "i");
            var s = ctx.Local(ctx.String, "s");
            var ss = ctx.Local(ctx.String, "ss");
            var c = ctx.Local(ctx.String, "c");
            var inc = ctx.Local(ctx.Boolean, "inc");
            var divMod10 = ctx.Local(ctx.Int64.MakeArray(), "divMod10");
            var valueDivMod10 = new ExprCall(ctx, (Func<UInt64, UInt64, object>)_Int64UInt64.UInt64DivRem, null, value.Expr, ctx.Literal(10L)).Named("valueDivMod10");
            var valueNeg = new ExprCall(ctx, (Func<Int64, Int64, Int64>)_Int64.Subtract, null, ctx.Literal(0L), value.Expr).Named("valueNeg");
            var sLen = ctx.Local(ctx.Int32, "sLen");
            var newFmtEx = new ExprNewObj(ctx, ctx.Module.Import(typeof(FormatException).GetConstructor(Type.EmptyTypes))).Named("newFmtEx");
            var js = @"
format = format || 'G';
precision = format.length > 1 ? +format.substr(1) : null;
fmt0 = format.charAt(0);
if (fmt0 === 'x' || fmt0 === 'X') {
    s = value[1].toString(16);
    if (value[0] !== 0) {
        s = value[0].toString(16) + Array(9 - s.length).join('0') + s;
    }
    if (fmt0 === 'X') {
        s = s.toUpperCase();
    }
    if (precision !== null && precision > s.length) {
        s = Array(precision - s.length + 1).join('0') + s;
    }
    return s;
}
if (value[0] === 0 && value[1] === 0) {
    s = '0';
} else if (value[0] === 0x80000000 && value[1] === 0) {
    s = '9223372036854775808';
    neg = true;
} else {
    if (value[0] >= 0x80000000) {
        value = valueNeg;
        neg = true;
    }
    s = '';
    while (value[0] !== 0 || value[1] !== 0) {
        divMod10 = valueDivMod10;
        value = divMod10[0];
        s = String.fromCharCode(48 + divMod10[1][1]) + s;
    }
}
switch (fmt0) {
case 'g':
case 'G':
    if (precision !== null && precision < s.length && precision > 0) {
        sLen = s.length;
        inc = +(s.charAt(precision)) >= 5;
        s = s.substr(0, precision);
        ss = '';
        for (i = precision - 1; i >= 0; i--) {
            c = s.charAt(i);
            if (inc) {
                c = (+c) + 1;
                if (c > 9) {
                    c = 0;
                } else {
                    inc = false;
                }
                ss = String.fromCharCode(48 + c) + ss;
            } else {
                ss = c + ss;
            }
        }
        if (inc) {
            sLen++;
            ss = '1' + ss.substr(0, ss.length - 1);
        }
        if (ss.length > 1) {
            ss = ss.charAt(0) + '.' + ss.substr(1);
        }
        s = ss.replace(/(\d)\.?0+$/, '$1');
        s += (fmt0 === 'g' ? 'e' : 'E') + '+' + (sLen - 1).toFixed().replace(/^(\d)$/, '0$1');;
    }
    break;
case 'd':
case 'D':
    if (precision !== null && precision > s.length) {
        s = Array(precision - s.length + 1).join('0') + s;
    }
    break;
case 'n':
case 'N':
    if (precision === null) precision = 2;
    s = s.replace(/(\d)(?=(\d{3})+$)/g, '$1,');
    if (precision > 0) {
        s += '.' + Array(precision + 1).join('0');
    }
    break;
default:
    throw newFmtEx;
}
return neg ? '-' + s : s;
";
            return new StmtJsExplicit(ctx, js, value, format, neg, precision, fmt0, i, s, ss, c, inc, divMod10, valueDivMod10, valueNeg, sLen, newFmtEx);
        }

        [JsRedirect]
        public static string FormatDouble(double value, string format, NumberFormatInfo info) {
            throw new JsImplException();
        }
        [Js]
        public static Stmt FormatDouble(Ctx ctx) {
            var value = ctx.MethodParameter(0, "value");
            var format = ctx.MethodParameter(1, "format");
            var newFmtEx = new ExprNewObj(ctx, ctx.Module.Import(typeof(FormatException).GetConstructor(Type.EmptyTypes))).Named("newFmtEx");
            var valuePos = ctx.Local(ctx.Int32, "valuePos");
            var precision = ctx.Local(ctx.Int32.MakeNullable(), "precision");
            var fmt0 = ctx.Local(ctx.String, "fmt0");
            var s = ctx.Local(ctx.String, "s");
            var parts = ctx.Local(ctx.String.MakeArray(), "parts");
            var i = ctx.Local(ctx.Int32, "i");
            var js = @"
if (isNaN(value)) return 'NaN';
if (value === Number.POSITIVE_INFINITY) return 'Infinity';
if (value === Number.NEGATIVE_INFINITY) return '-Infinity';
format = format || 'G';
valuePos = Math.abs(value);
precision = format.length > 1 ? +format.substr(1) : null;
fmt0 = format.charAt(0);
switch (fmt0) {
case 'g':
case 'G':
    s = valuePos.toPrecision(precision || 15);
    if (s.indexOf('e') >= 0) {
        parts = s.split('e');
        parts[0] = parts[0].replace(/(\d)\.?0+$/, '$1');
        parts[1] = parts[1].replace(/^\+(\d)$/, '+0$1');
        s = parts.join(fmt0 === 'g' ? 'e' : 'E');
    } else if (s.indexOf('.') >= 0) {
        s = s.replace(/(\d)\.?0+$/g, '$1');
    }
    break;
case 'f':
case 'F':
case 'n':
case 'N':
    s = valuePos.toExponential(14);
    parts = s.replace('.', '').split('e');
    i = Number(parts[1]) + 1;
    s = parts[0];
    if (i <= 0) {
        s = '0' + Array(-i + 1).join('0') + s;
        i = 1;
    }
    if (precision === null) precision = 2;
    i += precision;
    if (s.length <= i) {
        s += Array(i - s.length + 1).join('0');
    } else {
        var needInc = (+s.charAt(i)) >= 5;
        s = s.substr(0, i);
        var cs = s.split('');
        if (needInc) {
            for (var j = cs.length - 1; j >= 0; j--) {
                if (++cs[j] < 10) {
                    needInc = false;
                    break;
                }
                cs[j] = 0;
            }
            if (needInc) {
                cs.unshift(1);
                i++;
            }
        }
        s = cs.join('');
    }
    i -= precision;
    if (fmt0 === 'n' || fmt0 === 'N') {
        s = s.substr(0, i).replace(/(\d)(?=(\d{3})+$)/g, '$1,') + (precision > 0 ? '.' + s.substr(i) : '');
    } else {
        s = s.substr(0, i) + (precision > 0 ? '.' + s.substr(i) : '');
    }
    break;
default:
    throw newFmtEx;
}
return value < 0 ? '-' + s : s;
";
            return new StmtJsExplicit(ctx, js, value, format, newFmtEx, valuePos, precision, fmt0, s, parts, i);
        }

        [JsRedirect]
        public static int ParseInt32(string s) {
            throw new JsImplException();
        }
        [Js]
        public static Expr ParseInt32(ICall call) {
            var ctx = call.Ctx;
            var number = call.Arg(0, "number");
            return new ExprJsExplicit(ctx, "(+number)", ctx.Int32, number);
        }

    }
}
