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
                var js = "hi=a[0]+b[0];lo=a[1]+b[1];if(lo>=limit){lo-=limit;hi++;}if(hi>=limit)hi-=limit;return[hi,lo];";
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
                var js = "hi=a[0]-b[0];lo=a[1]-b[1];if(lo<0){lo+=limit;hi--;}if(hi<0)hi+=limit;return[hi,lo];";
                var stmt = new StmtJsExplicit(ctx, js, a, b, hi, lo, limit);
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

        [Js(typeof(MultiplyImpl))]
        public static Int64 Multiply(Int64 a, Int64 b) {
            throw new Exception();
        }

        class MultiplyImpl : IJsImpl {
            public Stmt GetImpl(Ctx ctx) {
                throw new NotImplementedException();
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

        [Js(typeof(MultiplyImpl))]
        public static UInt64 Multiply(UInt64 a, UInt64 b) {
            throw new Exception();
        }

        class MultiplyImpl : IJsImpl {
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
                var hi = ctx.Local(ctx._UInt64, "hi");
                var lo = ctx.Local(ctx._UInt64, "lo");
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
lo = rr[3] + rr[2] * limit;
hi = rr[1] + rr[0] * limit;
return [hi, lo];
";
                var stmt = new StmtJsExplicit(ctx, js, a, b, aa, bb, rr, hi, lo, mask, limit, ia, ib, mul, add, mulCarry, addCarry, rrOfs);
                return stmt;
            }
        }

    }

}
