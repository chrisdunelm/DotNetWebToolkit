using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.JsResolvers;
using DotNetWebToolkit.Cil2Js.Utils;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Output {

    class VisitorJsResolveConv : JsAstVisitor {

        public static ICode V(ICode ast) {
            var v = new VisitorJsResolveConv();
            return v.Visit(ast);
        }

        private static Expr NotImpl(ExprConv e) {
            throw new NotImplementedException();
        }

        private static Expr Iden(ExprConv e) {
            return e.Expr;
        }

        private static Expr CConv(ExprConv e) {
            var ctx = e.Ctx;
            var convMi = ((Func<int, int>)InternalFunctions.Conv<int, int>).Method.GetGenericMethodDefinition();
            var mConv = ctx.Module.Import(convMi).MakeGeneric(e.Expr.Type, e.Type);
            var convCall = new ExprCall(ctx, mConv, null, e.Expr);
            return convCall;
        }

        private static Expr I8_U8(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "((e<<24)>>>24)", e.Type, e.Expr.Named("e"));
        }

        private static Expr I8_U16(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(((e<<24)>>8)>>>16)", e.Type, e.Expr.Named("e"));
        }

        private static Expr I8_U32(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "((e<<24)>>24", e.Type, e.Expr.Named("e"));
            var ctx = e.Ctx;
            var limit = ctx.Literal(0x100000000UL, ctx._UInt64, "limit");
            return new ExprJsExplicit(e.Ctx, "(e<0?limit+e:e)", e.Type, e.Expr.Named("e"), limit);
        }

        private static Expr I16_I8(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "((a<<24)>>24)", e.Type, e.Expr.Named("a"));
        }

        private static Expr I16_U8(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "((a<<24)>>>24)", e.Type, e.Expr.Named("a"));
        }

        private static Expr I16_U16(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "((a<<16)>>>16)", e.Type, e.Expr.Named("a"));
        }

        private static Expr I16_U32(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "((a<<16)>>16)", e.Type, e.Expr.Named("a"));
        }

        private static Expr I32_I8(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "((a<<24)>>24)", e.Type, e.Expr.Named("a"));
        }

        private static Expr I32_U8(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "((a<<24)>>>24)", e.Type, e.Expr.Named("a"));
        }

        private static Expr I32_I16(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "((a<<16)>>16)", e.Type, e.Expr.Named("a"));
        }

        private static Expr I32_U16(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "((a<<16)>>>16)", e.Type, e.Expr.Named("a"));
        }

        private static Expr U16_I8(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "((a<<24)>>24)", e.Type, e.Expr.Named("a"));
        }

        private static Expr U16_U8(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(a&0xff)", e.Type, e.Expr.Named("a"));
        }

        private static Expr U16_I16(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "((a<<16)>>16)", e.Type, e.Expr.Named("a"));
        }

        private static Expr U32_I8(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "((a<<24)>>24)", e.Type, e.Expr.Named("a"));
        }

        private static Expr U32_U8(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(a&0xff)", e.Type, e.Expr.Named("a"));
        }

        private static Expr U32_I16(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "((a<<16)>>16)", e.Type, e.Expr.Named("a"));
        }

        private static Expr U32_U16(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(a&0xffff)", e.Type, e.Expr.Named("a"));
        }

        private static Expr U32_I32(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(~~a)", e.Type, e.Expr.Named("a"));
        }

        private static Expr Ix_X64(ExprConv e) {
            var ctx = e.Ctx;
            var max = ctx.Literal(0xffffffffU, ctx.UInt32, "max");
            var limit = ctx.Literal(0x100000000UL, ctx._UInt64, "limit");
            return new ExprJsExplicit(ctx, "(e<0?[max,limit+e]:[0,e])", e.Type, e.Expr.Named("e"), max, limit);
        }

        private static Expr Ux_X64(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "[0,e]", e.Type, e.Expr.Named("e"));
        }

        private static Expr R_I8(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "((a<<24)>>24)", e.Type, e.Expr.Named("a"));
        }

        private static Expr R_U8(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(a&0xff)", e.Type, e.Expr.Named("a"));
        }

        private static Expr R_I16(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "((a<<16)>>16)", e.Type, e.Expr.Named("a"));
        }

        private static Expr R_I32(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(~~a)", e.Type, e.Expr.Named("a"));
        }

        private static Expr R_U16(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(a&0xffff)", e.Type, e.Expr.Named("a"));
        }

        private static Expr I32_I64(ExprConv e) {
            var ctx = e.Ctx;
            var max = new ExprLiteral(ctx, 0xffffffffU, ctx.UInt32).Named("max");
            var js = "[a<0?max:0,a]";
            return new ExprJsExplicit(ctx, js, ctx.Int64, e.Expr.Named("a"), max);
        }

        private static Func<ExprConv, Expr>[,] convs =
        {  // --> To
           // Int8     Int16    Int32    Int64    UInt8    UInt16   UInt32   UInt64   Single   Double
            { Iden   , Iden   , Iden   , Ix_X64 , I8_U8  , I8_U16 , I8_U32 , Ix_X64 , Iden   , Iden    }, // Int8   |
            { I16_I8 , Iden   , Iden   , Ix_X64 , I16_U8 , I16_U16, CConv  , Ix_X64 , Iden   , Iden    }, // Int16  |
            { I32_I8 , I32_I16, Iden   , Ix_X64 , I32_U8 , I32_U16, CConv  , Ix_X64 , Iden   , Iden    }, // Int32  V
            { NotImpl, NotImpl, NotImpl, Iden   , NotImpl, NotImpl, NotImpl, Iden   , Iden   , Iden    }, // Int64  From
            { CConv  , Iden   , Iden   , Ux_X64 , Iden   , Iden   , Iden   , Ux_X64 , Iden   , Iden    }, // UInt8
            { U16_I8 , U16_I16, Iden   , Ux_X64 , U16_U8 , Iden   , Iden   , Ux_X64 , Iden   , Iden    }, // UInt16
            { U32_I8 , U32_I16, U32_I32, Ux_X64 , U32_U8 , U32_U16, Iden   , Ux_X64 , Iden   , Iden    }, // UInt32
            { NotImpl, NotImpl, NotImpl, Iden   , NotImpl, NotImpl, NotImpl, Iden   , Iden   , Iden    }, // UInt64
            { R_I8   , R_I16  , R_I32  , NotImpl, R_U8   , R_U16  , CConv  , NotImpl, Iden   , Iden    }, // Single
            { R_I8   , R_I16  , R_I32  , NotImpl, R_U8   , R_U16  , CConv  , NotImpl, Iden   , Iden    }, // Double
        };

        private Dictionary<string, Func<Ctx, Expr>> consts = new Dictionary<string, Func<Ctx, Expr>> {
            { "u32max", ctx => ctx.Literal(0xffffffff, ctx.UInt32) },
            { "u32limit", ctx => ctx.Literal(0x100000000, ctx._UInt64) },
        };

        private const string Identity = "e";
        private const string Ix_X64_ = "(e < 0 ? [u32max, u32limit + e] : [0, e])";

        private static string[,] jss =
        {
            { // Int8 ->
                Identity, // -> Int8
                Identity, // -> Int16
                Identity, // -> Int32
                Ix_X64_, // -> Int64
                "((e << 24) >>> 24)", // -> UInt8
                "(((e << 24) >> 8) >>> 16)" , // -> UInt16
                "(((e << 24) >> 24) >>> 0)", // -> UInt32
                Ix_X64_, // -> UInt64
                Identity, // -> Single
                Identity, // -> Double
            },
            { // Int16 ->
                "((e << 24) >> 24)", // -> Int8
                Identity, // -> Int16
                Identity, // -> Int32
                Ix_X64_, // -> Int64
                "((e << 24) >>> 24)", // -> UInt8
                "((e << 16) >>> 16)", // -> UInt16
                "(((e << 16) >> 16) >>> 0)", // -> UInt32
                Ix_X64_, // -> UInt64
                Identity, // -> Single
                Identity, // -> Double
            },
            { // Int32 ->
                "((e << 24) >> 24)", // -> Int8
                "((e << 16) >> 16)", // -> Int16
                Identity, // -> Int32
                Ix_X64_, // -> Int64
                "((e << 24) >>> 24)", // -> UInt8
                "((e << 16) >>> 16)", // -> UInt16
                "(e >>> 0)", // -> UInt32
                Ix_X64_, // -> UInt64
                Identity, // -> Single
                Identity, // -> Double
            },
            { // Int64 ->
                null, // -> Int8
                null, // -> Int16
                null, // -> Int32
                null, // -> Int64
                null, // -> UInt8
                null, // -> UInt16
                null, // -> UInt32
                null, // -> UInt64
                null, // -> Single
                null, // -> Double
            },
            { // UInt8 ->
                null, // -> Int8
                null, // -> Int16
                null, // -> Int32
                null, // -> Int64
                null, // -> UInt8
                null, // -> UInt16
                null, // -> UInt32
                null, // -> UInt64
                null, // -> Single
                null, // -> Double
            },
        };

        private static Dictionary<MetadataType, int> indexMap = new Dictionary<MetadataType, int> {
            { MetadataType.SByte, 0 },
            { MetadataType.Int16, 1 },
            { MetadataType.Int32, 2 },
            { MetadataType.Int64, 3 },
            { MetadataType.Byte, 4 },
            { MetadataType.UInt16, 5 },
            { MetadataType.UInt32, 6 },
            { MetadataType.UInt64, 7 },
            { MetadataType.Single, 8 },
            { MetadataType.Double, 9 },
        };

        protected override ICode VisitConv(ExprConv e) {
            var fromType = e.Expr.Type;
            var toType = e.Type;

            var fromTypeMetadataType = fromType.MetadataType;
            var toTypeMetadataType = toType.MetadataType;
            if (e.ForceFromUnsigned) {
                switch (fromTypeMetadataType) {
                case MetadataType.SByte: fromTypeMetadataType = MetadataType.Byte; break;
                case MetadataType.Int16: fromTypeMetadataType = MetadataType.UInt16; break;
                case MetadataType.Int32: fromTypeMetadataType = MetadataType.UInt32; break;
                case MetadataType.Int64: fromTypeMetadataType = MetadataType.UInt64; break;
                case MetadataType.IntPtr: fromTypeMetadataType = MetadataType.UIntPtr; break;
                }
            }

            var fromIdx = indexMap.ValueOrDefault(fromTypeMetadataType, -1);
            var toIdx = indexMap.ValueOrDefault(toTypeMetadataType, -1);

            if (fromIdx == -1 || toIdx == -1) {
                return e.Expr;
            }

            //var fn = convs[fromIdx, toIdx];
            //var ret = fn(e);
            //return ret;

            var ctx = e.Ctx;
            var js = jss[fromIdx, toIdx];
            var parameters = new List<NamedExpr> { e.Expr.Named("e")};
            foreach (var constant in consts) {
                if (js.Contains(constant.Key)) {
                    parameters.Add(constant.Value(ctx).Named(constant.Key));
                }
            }
            var expr = new ExprJsExplicit(ctx, js, e.Type, parameters);
            return expr;
        }

    }

}
