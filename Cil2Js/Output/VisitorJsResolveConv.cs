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

        private static Expr I8_U32(ExprConv e) {
            var ctx = e.Ctx;
            var limit = ctx.Literal(0x100000000UL, ctx._UInt64, "limit");
            return new ExprJsExplicit(e.Ctx, "(e<0?limit+e:e)", e.Type, e.Expr.Named("e"), limit);
        }

        //private static Expr U8_X64(ExprConv e) {
        //    var ctx = e.Ctx;
        //    return new ExprJsExplicit(e.Ctx, "[0,e]", e.Type, e.Expr.Named("e"));
        //}

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
            { Iden   , Iden   , Iden   , Ix_X64 , CConv  , CConv  , I8_U32 , Ix_X64 , Iden   , Iden    }, // Int8   |
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

            var fn = convs[fromIdx, toIdx];
            var ret = fn(e);

            return ret;
        }

    }

}
