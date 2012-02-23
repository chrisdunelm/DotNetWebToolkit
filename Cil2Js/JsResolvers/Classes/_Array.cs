using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    class _Array {

        [Js("Copy", typeof(void), typeof(Array), typeof(int), typeof(Array), typeof(int), typeof(int))]
        public static Expr Copy(ICall call) {
            // d = arg[0].slice(arg[1],arg[4]+arg[1])
            // Array.prototype.splice.apply(arg[2], [arg[3], arg[4]].concat(d))
            var ctx = call.Ctx;
            var src = call.Args.ElementAt(0);
            var srcIdx = call.Args.ElementAt(1);
            var dst = call.Args.ElementAt(2);
            var dstIdx = call.Args.ElementAt(3);
            var length = call.Args.ElementAt(4);
            var arrayPart = new ExprJsResolvedMethod(ctx, src.Type, src, "slice", srcIdx, ctx.ExprGen.Add(srcIdx, length));
            var spliceFixedArgs = new ExprJsArrayLiteral(ctx, ctx.Object, dstIdx, length);
            var spliceArgs = new ExprJsResolvedMethod(ctx, spliceFixedArgs.Type, spliceFixedArgs, "concat", arrayPart);
            var copy = new ExprJsResolvedMethod(ctx, ctx.Void, null, "Array.prototype.splice.apply", dst, spliceArgs);
            return copy;
        }

        [Js]
        public static Stmt Clear(Ctx ctx) {
            var array = ctx.MethodParameter(0);
            var index = ctx.MethodParameter(1).Named("index");
            var length = ctx.MethodParameter(2).Named("length");
            var arrayElementType = array.Type.GetElementType();
            var i = new ExprVarLocal(ctx, ctx.Int32).Named("i");
            var value = new ExprDefaultValue(ctx, arrayElementType).Named("value");
            var js = "for (i=0; i<length; i++) a[index+i]=value;";
            var stmt = new StmtJsExplicit(ctx, js, i, length, array.Named("a"), index, value);
            return stmt;
        }

        [Js]
        public static Expr get_Length(ICall call) {
            // This is only when Array.Length is called. Typed arrays (1 dimensional) use a specific CIL opcode to get length
            var ctx = call.Ctx;
            return new ExprJsExplicit(ctx, "array.length", ctx.Int32, call.Obj.Named("array"));
        }

        // Not yet supported - problem with how to box value-types in arrays
        //[Js(typeof(object), typeof(int))]
        //public static Stmt GetValue(Ctx ctx) {
        //    var arrayType = new ExprCall(ctx, (Func<Type>)(new object().GetType), ctx.This).Named("arrayType");
        //    var elTypeData = new ExprJsTypeData(ctx, TypeData.ElementType).Named("elTypeData");
        //    var elType = new ExprJsExplicit(ctx, "arrayType[elTypeData]", ctx.Type, arrayType, elTypeData);
        //    var getValue = new ExprJsExplicit(ctx, "this[index]", ctx.Object, ctx.ThisNamed, ctx.MethodParameter(0, "index"));
        //    var box = new ExprBox(ctx, getValue, ctx.Object);
        //    return new StmtReturn(ctx, box);
        //}

        public static int IndexOf<T>(T[] array, T value) {
            return Array.IndexOf<T>(array, value, 0, array.Length);
        }

        public static int IndexOf<T>(T[] array, T value, int startIndex) {
            return Array.IndexOf<T>(array, value, startIndex, array.Length - startIndex);
        }

        public static int IndexOf<T>(T[] array, T value, int startIndex, int count) {
            var end = startIndex + count;
            if (value != null) {
                for (int i = startIndex; i < end; i++) {
                    if (value.Equals(array[i])) {
                        return i;
                    }
                }
            } else {
                for (int i = startIndex; i < end; i++) {
                    if (array[i] == null) {
                        return i;
                    }
                }
            }
            return -1;
        }

        public static int IndexOf(Array array, object value) {
            return Array.IndexOf(array, value, 0, array.Length);
        }

        public static int IndexOf(Array array, object value, int startIndex) {
            return Array.IndexOf(array, value, startIndex, array.Length - startIndex);
        }

        public static int IndexOf(Array array, object value, int startIndex, int count) {
            var end = startIndex + count;
            if (value != null) {
                for (int i = startIndex; i < end; i++) {
                    if (value.Equals(array.GetValue(i))) {
                        return i;
                    }
                }
            } else {
                for (int i = startIndex; i < end; i++) {
                    if (array.GetValue(i) == null) {
                        return i;
                    }
                }
            }
            return -1;
        }

        [Js("Sort", typeof(void), typeof(GenMethodParam0[]))]
        public static Expr SortArray(ICall call) {
            var ctx = call.Ctx;
            var genType = call.CallMethod.GetGenericArgument(0);
            if (genType.IsNumeric()) {
                var js = "array.sort(function(a, b) { return a - b; })";
                return new ExprJsExplicit(ctx, js, ctx.Void, call.Args.ElementAt(0).Named("array"));
            }
            throw new NotImplementedException("Cannot handle non-numeric array sorts yet");
        }

        [Js("Sort", typeof(void), typeof(GenMethodParam0[]), typeof(int), typeof(int))]
        public static Stmt SortWithRange(Ctx ctx) {
            var genType = ctx.MRef.GetGenericArgument(0);
            if (genType.IsNumeric()) {
                var array = ctx.MethodParameter(0, "array");
                var index = ctx.MethodParameter(1, "index");
                var length = ctx.MethodParameter(2, "length");
                var part = ctx.Local(genType.MakeArray(), "part");
                var mSortGenDef = ctx.Module.Import(((Action<GenMethodParam0[]>)Array.Sort).Method.GetGenericMethodDefinition());
                var mSortCall = mSortGenDef.MakeGeneric(genType);
                var sortPartCall = new ExprCall(ctx, mSortCall, null, part.Expr).Named("sortPartCall");
                var copyPartCall = new ExprCall(ctx, (Action<Array, int, Array, int, int>)Array.Copy, null, part.Expr, ctx.Literal(0), array.Expr, index.Expr, length.Expr).Named("copyPartCall");
                var js = @"
part = array.slice(index, index + length);
sortPartCall;
copyPartCall;
";
                return new StmtJsExplicit(ctx, js, array, index, length, part, sortPartCall, copyPartCall);
            }
            throw new NotImplementedException("Cannot handle non-numeric array sorts yet");
        }

        class C : IComparer<int> {
            public int Compare(int x, int y) {
                throw new NotImplementedException();
            }
        }
        [Js("Sort", typeof(void), typeof(GenMethodParam0[]), typeof(IComparer<GenMethodParam0>))]
        public static Expr SortArrayComparer(ICall call) {
            var ctx = call.Ctx;
            var genType = call.CallMethod.GetGenericArgument(0);
            var array = call.Arg(0, "array");
            var comparer = call.Arg(1);
            var mCompare = comparer.Type.EnumResolvedMethods().First(x => x.Name == "Compare");
            var a = ctx.Local(genType, "a");
            var b = ctx.Local(genType, "b");
            var comparerCall = new ExprCall(ctx, mCompare, comparer, new[] { a.Expr, b.Expr }, true).Named("comparerCall");
            var js = "array.sort(function(a, b) { return comparerCall; })";
            return new ExprJsExplicit(ctx, js, ctx.Void, array, comparerCall, a, b);
        }

        public static int BinarySearch<T>(T[] array, T value) {
            return Array.BinarySearch<T>(array, 0, array.Length, value, null);
        }

        public static int BinarySearch<T>(T[] array, T value, IComparer<T> comparer) {
            return Array.BinarySearch<T>(array, 0, array.Length, value, comparer);
        }

        public static int BinarySearch<T>(T[] array, int index, int length, T value, IComparer<T> comparer) {
            if (comparer == null) {
                comparer = Comparer<T>.Default;
            }
            for (; ; ) {
                var mid = index + length / 2;
                var comp = comparer.Compare(value, array[mid]);
                if (comp == 0) {
                    return mid;
                }
                if (length <= 1) {
                    while (comparer.Compare(value, array[mid]) > 0) { // value > array[mid]
                        mid++;
                        if (mid >= array.Length) {
                            break;
                        }
                    }
                    return ~mid;
                }
                if (comp < 0) {
                    // value < array[mid]
                    length = mid - index;
                } else {
                    // value > array[mid]
                    length -= mid + 1 - index;
                    index = mid + 1;
                }
            }
        }

    }
}
