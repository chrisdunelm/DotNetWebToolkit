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

        private static Func<ExprConv, Expr>[,] convs =
        {  // --> To
           // SByte    Byte     Int16    Int32    Int64    UInt16   UInt32   UInt64   Single   Double
            { Iden   , CConv  , Iden   , Iden   , Iden   , CConv  , CConv  , CConv  , Iden   , Iden    }, // SByte  |
            { CConv  , Iden   , Iden   , Iden   , Iden   , Iden   , Iden   , Iden   , Iden   , Iden    }, // Byte   |
            { I16_I8 , I16_U8 , Iden   , Iden   , Iden   , I16_U16, CConv  , CConv  , Iden   , Iden    }, // Int16  V
            { I32_I8 , I32_U8 , I32_I16, Iden   , Iden   , I32_U16, CConv  , CConv  , Iden   , Iden    }, // Int32  From
            { NotImpl, NotImpl, NotImpl, NotImpl, Iden   , NotImpl, NotImpl, CConv  , Iden   , Iden    }, // Int64
            { NotImpl, NotImpl, NotImpl, NotImpl, NotImpl, Iden   , NotImpl, NotImpl, Iden   , Iden    }, // UInt16
            { NotImpl, NotImpl, NotImpl, NotImpl, NotImpl, NotImpl, Iden   , NotImpl, Iden   , Iden    }, // UInt32
            { NotImpl, NotImpl, NotImpl, NotImpl, CConv  , NotImpl, CConv  , Iden   , Iden   , Iden    }, // UInt64
            { NotImpl, NotImpl, NotImpl, NotImpl, NotImpl, NotImpl, NotImpl, NotImpl, Iden   , Iden    }, // Single
            { NotImpl, NotImpl, NotImpl, NotImpl, NotImpl, NotImpl, NotImpl, NotImpl, Iden   , Iden    }, // Double
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

            var fromIdx = indexMap[fromType.MetadataType];
            var toIdx = indexMap[toType.MetadataType];

            var fn = convs[fromIdx, toIdx];
            var ret = fn(e);

            return ret;
        }

    }

}
