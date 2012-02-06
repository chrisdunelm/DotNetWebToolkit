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

        private const string Identity = "e";
        private const string CallConv = null;

        private static string[,] jss =
        {
            { // Int8 ->
                Identity, // -> Int8
                Identity, // -> Int16
                Identity, // -> Int32
                CallConv, // -> Int64
                "((e << 24) >>> 24)", // -> UInt8
                "(((e << 24) >> 8) >>> 16)" , // -> UInt16
                "(((e << 24) >> 24) >>> 0)", // -> UInt32
                CallConv, // -> UInt64
                Identity, // -> Single
                Identity, // -> Double
            },
            { // Int16 ->
                "((e << 24) >> 24)", // -> Int8
                Identity, // -> Int16
                Identity, // -> Int32
                CallConv, // -> Int64
                "((e << 24) >>> 24)", // -> UInt8
                "((e << 16) >>> 16)", // -> UInt16
                "(((e << 16) >> 16) >>> 0)", // -> UInt32
                CallConv, // -> UInt64
                Identity, // -> Single
                Identity, // -> Double
            },
            { // Int32 ->
                "((e << 24) >> 24)", // -> Int8
                "((e << 16) >> 16)", // -> Int16
                Identity, // -> Int32
                CallConv, // -> Int64
                "(e & 0xff)", // -> UInt8
                "(e & 0xffff)", // -> UInt16
                "(e >>> 0)", // -> UInt32
                CallConv, // -> UInt64
                Identity, // -> Single
                Identity, // -> Double
            },
            { // Int64 ->
                "((e[1] << 24) >> 24)", // -> Int8
                "((e[1] << 16) >> 16)", // -> Int16
                "(~~e[1])", // -> Int32
                Identity, // -> Int64
                "(e[1] & 0xff)", // -> UInt8
                "(e[1] & 0xffff)", // -> UInt16
                "(e[1] >>> 0)", // -> UInt32
                Identity, // -> UInt64
                CallConv, // -> Single
                CallConv, // -> Double
            },
            { // UInt8 ->
                "((e << 24) >> 24)", // -> Int8
                Identity, // -> Int16
                Identity, // -> Int32
                "[0, e]", // -> Int64
                Identity, // -> UInt8
                Identity, // -> UInt16
                Identity, // -> UInt32
                "[0, e]", // -> UInt64
                Identity, // -> Single
                Identity, // -> Double
            },
            { // UInt16 ->
                "((e << 24) >> 24)", // -> Int8
                "((e << 16) >> 16)", // -> Int16
                Identity, // -> Int32
                "[0, e]", // -> Int64
                "(e & 0xff)", // -> UInt8
                Identity, // -> UInt16
                Identity, // -> UInt32
                "[0, e]", // -> UInt64
                Identity, // -> Single
                Identity, // -> Double
            },
            { // UInt32 ->
                "((e << 24) >> 24)", // -> Int8
                "((e << 16) >> 16)", // -> Int16
                "(~~e)", // -> Int32
                "[0, e]", // -> Int64
                "(e & 0xff)", // -> UInt8
                "(e & 0xffff)", // -> UInt16
                Identity, // -> UInt32
                "[0, e]", // -> UInt64
                Identity, // -> Single
                Identity, // -> Double
            },
            { // UInt64 ->
                "((e[1] << 24) >> 24)", // -> Int8
                "((e[1] << 16) >> 16)", // -> Int16
                "(~~e[1])", // -> Int32
                Identity, // -> Int64
                "(e[1] & 0xff)", // -> UInt8
                "(e[1] & 0xffff)", // -> UInt16
                "e[1]", // -> UInt32
                Identity, // -> UInt64
                CallConv, // -> Single
                CallConv, // -> Double
            },
            { // Single ->
                "((e << 24) >> 24)", // -> Int8
                "((e << 16) >> 16)", // -> Int16
                "(~~e)", // -> Int32
                CallConv, // -> Int64
                "(e & 0xff)", // -> UInt8
                "(e & 0xffff)", // -> UInt16
                "(e >>> 0)", // -> UInt32
                CallConv, // -> UInt64
                Identity, // -> Single
                Identity, // -> Double
            },
            { // Double ->
                "((e << 24) >> 24)", // -> Int8
                "((e << 16) >> 16)", // -> Int16
                "(~~e)", // -> Int32
                CallConv, // -> Int64
                "(e & 0xff)", // -> UInt8
                "(e & 0xffff)", // -> UInt16
                "(e >>> 0)", // -> UInt32
                CallConv, // -> UInt64
                Identity, // -> Single
                Identity, // -> Double
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
            if (fromTypeMetadataType == MetadataType.Char) {
                fromTypeMetadataType = MetadataType.UInt16;
            }
            if (toTypeMetadataType == MetadataType.Char) {
                toTypeMetadataType = MetadataType.UInt16;
            }

            var fromIdx = indexMap.ValueOrDefault(fromTypeMetadataType, -1);
            var toIdx = indexMap.ValueOrDefault(toTypeMetadataType, -1);

            if (fromIdx == -1 || toIdx == -1) {
                return e.Expr;
            }

            var ctx = e.Ctx;
            var js = jss[fromIdx, toIdx];
            if (js != null) {
                // In-place conversion
                var expr = new ExprJsExplicit(ctx, js, e.Type, e.Expr.Named("e"));
                return expr;
            } else {
                // Call conversion function
                var convMi = ((Func<int, int>)InternalFunctions.Conv<int, int>).Method.GetGenericMethodDefinition();
                var mConv = ctx.Module.Import(convMi).MakeGeneric(e.Expr.Type, e.Type);
                var convCall = new ExprCall(ctx, mConv, null, e.Expr);
                return convCall;
            }
        }

    }

}
