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

        private static Expr DblNot(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(~~{0})", e.Type, e.Expr);
        }

        private static Expr CConv(ExprConv e) {
            var ctx = e.Ctx;
            var convMi = ((Func<int, int>)InternalFunctions.Conv<int, int>).Method.GetGenericMethodDefinition();
            var mConv = ctx.Module.Import(convMi).MakeGeneric(e.Expr.Type, e.Type);
            var convCall = new ExprCall(ctx, mConv, null, e.Expr);
            return convCall;
        }

        private static Expr I16_I8(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(({0}<<24)>>24)", e.Type, e.Expr);
        }

        private static Expr I16_U8(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(({0}<<24)>>>24)", e.Type, e.Expr);
        }

        private static Expr I16_U16(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(({0}<<16)>>>16)", e.Type, e.Expr);
        }

        private static Expr I16_U32(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(({0}<<16)>>16)", e.Type, e.Expr);
        }

        private static Expr I32_I8(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(({0}<<24)>>24)", e.Type, e.Expr);
        }

        private static Expr I32_U8(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(({0}<<24)>>>24)", e.Type, e.Expr);
        }

        private static Expr I32_I16(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(({0}<<16)>>16)", e.Type, e.Expr);
        }

        private static Expr I32_U16(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(({0}<<16)>>>16)", e.Type, e.Expr);
        }

        private static Expr U16_I8(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(({0}<<24)>>24)", e.Type, e.Expr);
        }

        private static Expr U16_U8(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "({0}&0xff)", e.Type, e.Expr);
        }

        private static Expr U16_I16(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(({0}<<16)>>16)", e.Type, e.Expr);
        }

        private static Expr U32_I8(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(({0}<<24)>>24)", e.Type, e.Expr);
        }

        private static Expr U32_U8(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "({0}&0xff)", e.Type, e.Expr);
        }

        private static Expr U32_I16(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(({0}<<16)>>16)", e.Type, e.Expr);
        }

        private static Expr U32_U16(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "({0}&0xffff)", e.Type, e.Expr);
        }

        private static Expr R_I8(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(({0}<<24)>>24)", e.Type, e.Expr);
        }

        private static Expr R_U8(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "({0}&0xff)", e.Type, e.Expr);
        }

        private static Expr R_I16(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(({0}<<16)>>16)", e.Type, e.Expr);
        }

        private static Expr R_I32(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "(~~{0})", e.Type, e.Expr);
        }

        private static Expr R_U16(ExprConv e) {
            return new ExprJsExplicit(e.Ctx, "({0}&0xffff)", e.Type, e.Expr);
        }

        private static Func<ExprConv, Expr>[,] convs =
        {  // --> To
           // SByte    Byte     Int16    Int32    Int64    UInt16   UInt32   UInt64   Single   Double
            { Iden   , CConv  , Iden   , Iden   , Iden   , CConv  , CConv  , CConv  , Iden   , Iden    }, // SByte  |
            { CConv  , Iden   , Iden   , Iden   , Iden   , Iden   , Iden   , Iden   , Iden   , Iden    }, // Byte   |
            { I16_I8 , I16_U8 , Iden   , Iden   , Iden   , I16_U16, CConv  , CConv  , Iden   , Iden    }, // Int16  V
            { I32_I8 , I32_U8 , I32_I16, Iden   , Iden   , I32_U16, CConv  , CConv  , Iden   , Iden    }, // Int32  From
            { NotImpl, NotImpl, NotImpl, NotImpl, NotImpl, NotImpl, NotImpl, NotImpl, Iden   , Iden    }, // Int64
            { U16_I8 , U16_U8 , U16_I16, Iden   , Iden   , Iden   , Iden   , Iden   , Iden   , Iden    }, // UInt16
            { U32_I8 , U32_U8 , U32_I16, DblNot , Iden   , U32_U16, Iden   , Iden   , Iden   , Iden    }, // UInt32
            { NotImpl, NotImpl, NotImpl, NotImpl, NotImpl, NotImpl, NotImpl, Iden   , Iden   , Iden    }, // UInt64
            { R_I8   , R_U8   , R_I16  , R_I32  , NotImpl, R_U16  , CConv  , NotImpl, Iden   , Iden    }, // Single
            { R_I8   , R_U8   , R_I16  , R_I32  , NotImpl, R_U16  , CConv  , NotImpl, Iden   , Iden    }, // Double
        };

        private static Dictionary<MetadataType, int> indexMap = new Dictionary<MetadataType, int> {
            { MetadataType.SByte, 0 },
            { MetadataType.Byte, 1 },
            { MetadataType.Int16, 2 },
            { MetadataType.Int32, 3 },
            { MetadataType.Int64, 4 },
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
