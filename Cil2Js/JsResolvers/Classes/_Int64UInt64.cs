using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    static class _Int64UInt64 {

        public class AddImpl : IJsImpl {
            public Stmt GetImpl(Ctx ctx) {
                var a = ctx.MethodParameter(0).Named("a");
                var b = ctx.MethodParameter(1).Named("b");
                var hi = ctx.Local(ctx.UInt64, "hi");
                var lo = ctx.Local(ctx.UInt64, "lo");
                var limit = ctx.Literal(0x100000000UL, ctx._UInt64, "limit");
                var js = @"
hi = a[0] + b[0];
lo = a[1] + b[1];
if (lo >= limit) {
    lo -= limit;
    hi++;
}
if (hi >= limit) hi -= limit;
return [hi, lo];
";
                var stmt = new StmtJsExplicit(ctx, js, a, b, hi, lo, limit);
                return stmt;
            }
        }

        public class SubtractImpl : IJsImpl {
            public Stmt GetImpl(Ctx ctx) {
                var a = ctx.MethodParameter(0, "a");
                var b = ctx.MethodParameter(1, "b");
                var hi = ctx.Local(ctx.UInt64, "hi");
                var lo = ctx.Local(ctx.UInt64, "lo");
                var limit = ctx.Literal(0x100000000UL, ctx._UInt64, "limit");
                var js = @"
hi = a[0] - b[0];
lo = a[1] - b[1];
if (lo < 0) {
    lo += limit;
    hi--;
}
if (hi < 0) hi += limit;
return [hi, lo];
";
                var stmt = new StmtJsExplicit(ctx, js, a, b, hi, lo, limit);
                return stmt;
            }

        }

        public class MultiplyImpl : IJsImpl {
            public Stmt GetImpl(Ctx ctx) {
                var a = ctx.MethodParameter(0, "a");
                var b = ctx.MethodParameter(1, "b");
                var aa = ctx.Local(ctx.Int32.MakeArray(), "aa");
                var bb = ctx.Local(ctx.Int32.MakeArray(), "bb");
                var ia = ctx.Local(ctx.Int32, "ia");
                var ib = ctx.Local(ctx.Int32, "ib");
                var mul = ctx.Local(ctx.Int32, "mul");
                var add = ctx.Local(ctx.Int32, "add");
                var mulCarry = ctx.Local(ctx.Int32, "mulCarry");
                var addCarry = ctx.Local(ctx.Int32, "addCarry");
                var rrOfs = ctx.Local(ctx.Int32, "rrOfs");
                var rr = ctx.Local(ctx.Int32.MakeArray(), "rr");
                var mask = ctx.Literal(0xffff, ctx.Int32, "mask");
                var limit = ctx.Literal(0x10000, ctx.Int32, "limit");
                var js = @"
aa = [a[0] >>> 16, a[0] & mask, a[1] >>> 16, a[1] & mask];
bb = [b[0] >>> 16, b[0] & mask, b[1] >>> 16, b[1] & mask];
rr = [0, 0, 0, 0];
for (ib = 3; ib >= 0; ib--) {
    mulCarry = 0;
    addCarry = 0;
    for(ia = 3; ia >= 3 - ib; ia--) {
        rrOfs = ia + ib - 3;
        mul = aa[ia] * bb[ib] + mulCarry;
        mulCarry = mul >>> 16;
        add = rr[rrOfs] + (mul & mask) + addCarry;
        if (add >= limit){
            rr[rrOfs] = add - limit;
            addCarry = 1;
        } else {
            rr[rrOfs] = add;
            addCarry = 0;
        }
    }
}
return [rr[1] + rr[0] * limit, rr[3] + rr[2] * limit];
";
                var stmt = new StmtJsExplicit(ctx, js, a, b, aa, bb, rr, mask, limit, ia, ib, mul, add, mulCarry, addCarry, rrOfs);
                return stmt;
            }
        }

        [Js(typeof(UInt64DivRemImpl))]
        public static object UInt64DivRem(UInt64 a, UInt64 b) {
            throw new Exception();
        }

        class UInt64DivRemImpl : IJsImpl {
            public Stmt GetImpl(Ctx ctx) {
                var a = ctx.MethodParameter(0, "a");
                var b = ctx.MethodParameter(1, "b");
                var u = ctx.Local(ctx.Int32.MakeArray(), "u");
                var v = ctx.Local(ctx.Int32.MakeArray(), "v");
                var m = ctx.Local(ctx.Int32, "m");
                var n = ctx.Local(ctx.Int32, "n");
                var r = ctx.Local(ctx.Int32.MakeArray(), "r");
                var q = ctx.Local(ctx.Int32.MakeArray(), "q");
                var i = ctx.Local(ctx.Int32, "i");
                var j = ctx.Local(ctx.Int32, "j");
                var k = ctx.Local(ctx.Int32, "k");
                var l = ctx.Local(ctx.Int32, "l");
                var p = ctx.Local(ctx.Int32, "p");
                var t = ctx.Local(ctx.Int32, "t");
                var s = ctx.Local(ctx.Int32, "s");
                var x = ctx.Local(ctx.Int32, "x");
                var qhat = ctx.Local(ctx.Int32, "qhat");
                var rhat = ctx.Local(ctx.Int32, "rhat");
                var mask = ctx.Literal(0xffff, ctx.Int32, "mask");
                var limit = ctx.Literal(0x10000, ctx.Int32, "limit");
                var divByZeroEx = new ExprNewObj(ctx, ctx.Module.Import(typeof(DivideByZeroException).GetConstructor(Type.EmptyTypes))).Named("divByZeroEx");
                var js = @"
v = [b[1] & mask, b[1] >>> 16, b[0] & mask, b[0] >>> 16];
while (v[v.length - 1] === 0) {
    v = v.slice(0, -1);
    if (v.length === 0) throw divByZeroEx;
}
n = v.length;
u = [a[1] & mask, a[1] >>> 16, a[0] & mask, a[0] >>> 16];
while (u[u.length - 1] === 0 && u.length > n) u = u.slice(0, -1);
m = u.length;
r = [0, 0, 0, 0];
q = [0, 0, 0, 0];
if (n === 1) {
    k = 0;
    for (j = m - 1; j >= 0; j--) {
        l = k * limit + u[j]
        q[j] = (l / v[0]) >>> 0;
        k = l - q[j] * v[0];
    }
    r[0] = k;
} else {
    x = v[n - 1];
    s = 16;
    while (x > 0) {
        x >>>= 1;
        s--;
    }
    l = 0;
    for (i = n - 1; i > 0; i--) v[i] = ((v[i] << s) | (v[i - 1] >> (16 - s))) & mask;
    v[0] = (v[0] << s) & mask;
    u.push(0);
    for (i = m; i> 0; i--) u[i] = ((u[i] << s) | (u[i - 1] >> (16 - s))) & mask;
    u[0] = (u[0] << s) & mask;
    for (j = m - n; j >= 0; j--) {
        l = u[j + n] * limit + u[j + n - 1];
        qhat = (l / v[n - 1]) >>> 0;
        rhat = l - qhat * v[n - 1];
        for (;;) {
            if (qhat >= limit || qhat * v[n - 2] > limit * rhat + u[j + n - 2]) {
                qhat--;
                rhat += v[n - 1];
                if (rhat < limit) continue;
            }
            break;
        }
        k = 0;
        for (i = 0; i < n; i++) {
            p = qhat * v[i];
            t = u[i + j] - k - (p & mask);
            u[i + j] = t & mask;
            k = (p >> 16) - (t >> 16);
        }
        t = u[j + n] - k;
        u[j + n] = t;
        q[j] = qhat;
        if (t < 0) {
            q[j]--;
            k = 0;
            for (i = 0; i < n; i++) {
                t = u[i + j] + n[i] + k;
                u[i + j] = t & mask;
                k = t >> 16;
            }
            u[j + n] += k;
        }
    }
    for (i = 0; i< n; i++) {
        r[i] = ((u[i] >> s) | (u[i + 1] << (16 - s))) & mask;
    }
}
return [[q[2] + q[3] * limit, q[0] + q[1] * limit], [r[2] + r[3] * limit, r[0] + r[1] * limit]];
";
                var stmt = new StmtJsExplicit(ctx, js, a, b, u, v, m, n, r, q, i, j, k, l, p, s, t, qhat, rhat, mask, limit, divByZeroEx);
                return stmt;
            }
        }

    }

    class _Int64 {

        [Js(typeof(_Int64UInt64.AddImpl))]
        public static Int64 Add(Int64 a, Int64 b) {
            throw new Exception();
        }

        [Js(typeof(_Int64UInt64.SubtractImpl))]
        public static Int64 Subtract(Int64 a, Int64 b) {
            throw new Exception();
        }

        [Js(typeof(_Int64UInt64.MultiplyImpl))]
        public static Int64 Multiply(Int64 a, Int64 b) {
            throw new Exception();
        }

        [Js(typeof(DivideImpl))]
        public static Int64 Divide(Int64 a, Int64 b) {
            throw new Exception();
        }

        class DivideImpl : IJsImpl {
            public Stmt GetImpl(Ctx ctx) {
                var a = ctx.MethodParameter(0, "a");
                var b = ctx.MethodParameter(1, "b");
                var aNegate = new ExprUnary(ctx, UnaryOp.Negate, ctx.Int64, a.Expr).Named("aNegate");
                var bNegate = new ExprUnary(ctx, UnaryOp.Negate, ctx.Int64, b.Expr).Named("bNegate");
                var divMod = new ExprCall(ctx, (Func<UInt64, UInt64, object>)_Int64UInt64.UInt64DivRem, null, a.Expr, b.Expr).Named("divMod");
                var r = ctx.Local(ctx.Int64, "r");
                var rNegate = new ExprUnary(ctx, UnaryOp.Negate, ctx.Int64, r.Expr).Named("rNegate");
                // TODO: Throw ArithmeticException if a == Int64.MinValue and b == -1
                // TODO: Handle a or b being Int64.MinValue
                var js = @"
neg = false;
if (a[0] >>> 31) {
    neg = !neg;
    a = aNegate;
}
if (b[0] >>> 31) {
    neg = !neg;
    b = bNegate;
}
r = divMod[0];
return neg ? rNegate : r;
";
                var stmt = new StmtJsExplicit(ctx, js, a, b, r, aNegate, bNegate, divMod, rNegate);
                return stmt;
            }
        }

        [Js(typeof(RemainderImpl))]
        public static Int64 Remainder(Int64 a, Int64 b) {
            throw new Exception();
        }

        class RemainderImpl : IJsImpl {
            public Stmt GetImpl(Ctx ctx) {
                var a = ctx.MethodParameter(0, "a");
                var b = ctx.MethodParameter(1, "b");
                var aNegate = new ExprUnary(ctx, UnaryOp.Negate, ctx.Int64, a.Expr).Named("aNegate");
                var bNegate = new ExprUnary(ctx, UnaryOp.Negate, ctx.Int64, b.Expr).Named("bNegate");
                var divMod = new ExprCall(ctx, (Func<UInt64, UInt64, object>)_Int64UInt64.UInt64DivRem, null, a.Expr, b.Expr).Named("divMod");
                var r = ctx.Local(ctx.Int64, "r");
                var rNegate = new ExprUnary(ctx, UnaryOp.Negate, ctx.Int64, r.Expr).Named("rNegate");
                // TODO: Throw ArithmeticException if a == Int64.MinValue and b == -1
                // TODO: Handle a or b being Int64.MinValue
                var js = @"
neg = false;
if (a[0] >>> 31) {
    a = aNegate;
    neg = true;
}
if (b[0] >>> 31) b = bNegate;
r = divMod[1];
return neg ? rNegate : r;
";
                var stmt = new StmtJsExplicit(ctx, js, a, b, r, aNegate, bNegate, divMod, rNegate);
                return stmt;
            }
        }

    }

    class _UInt64 {

        [Js(typeof(_Int64UInt64.AddImpl))]
        public static UInt64 Add(UInt64 a, UInt64 b) {
            throw new Exception();
        }

        [Js(typeof(_Int64UInt64.SubtractImpl))]
        public static UInt64 Subtract(UInt64 a, UInt64 b) {
            throw new Exception();
        }

        [Js(typeof(_Int64UInt64.MultiplyImpl))]
        public static UInt64 Multiply(UInt64 a, UInt64 b) {
            throw new Exception();
        }

        [Js(typeof(DivideImpl))]
        public static UInt64 Divide(UInt64 a, UInt64 b) {
            throw new Exception();
        }

        class DivideImpl : IJsImpl {
            public Stmt GetImpl(Ctx ctx) {
                var a = ctx.MethodParameter(0);
                var b = ctx.MethodParameter(1);
                var divMod = new ExprCall(ctx, (Func<UInt64, UInt64, object>)_Int64UInt64.UInt64DivRem, null, a, b).Named("divMod");
                var js = "return divMod[0];";
                var stmt = new StmtJsExplicit(ctx, js, divMod);
                return stmt;
            }
        }

        [Js(typeof(RemainderImpl))]
        public static UInt64 Remainder(UInt64 a, UInt64 b) {
            throw new Exception();
        }

        class RemainderImpl : IJsImpl {
            public Stmt GetImpl(Ctx ctx) {
                var a = ctx.MethodParameter(0);
                var b = ctx.MethodParameter(1);
                var divMod = new ExprCall(ctx, (Func<UInt64, UInt64, object>)_Int64UInt64.UInt64DivRem, null, a, b).Named("divMod");
                var js = "return divMod[1];";
                var stmt = new StmtJsExplicit(ctx, js, divMod);
                return stmt;
            }
        }

    }

}
