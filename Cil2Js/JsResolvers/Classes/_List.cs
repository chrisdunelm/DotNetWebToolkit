using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Attributes;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;
using DotNetWebToolkit.Cil2Js.Utils;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    class _List<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable {

        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable {

            internal Enumerator(_List<T> list)
                : this() {
                this.list = list;
                this.ofs = -1;
            }

            private _List<T> list;
            private int ofs;

            public T Current {
                get { return this.list[this.ofs]; }
            }

            public void Dispose() {
            }

            object IEnumerator.Current {
                get { return this.list[this.ofs]; }
            }

            public bool MoveNext() {
                this.ofs++;
                return this.ofs < this.list.Count;
            }

            void IEnumerator.Reset() {
                this.ofs = -1;
            }
        }

        public _List() {
            array = new T[0];
        }

        public _List(int capacity) {
            array = new T[0];
        }

        public _List(IEnumerable<T> collection)
            : this() {
            this.AddRange(collection);
        }

        private T[] array;

        private static NamedExpr GetNamedArrayField(ICall call) {
            var ctx = call.Ctx;
            var arrayFieldRef = ctx.Module.Import(typeof(_List<T>)).EnumResolvedFields().First();
            return new ExprFieldAccess(ctx, call.Obj, arrayFieldRef).Named("array");
        }

        [JsRedirect(typeof(List<>))]
        private void Insert(int index, T item) { throw new JsImplException(); }
        [Js]
        public static Expr Insert(ICall call) {
            var ctx = call.Ctx;
            var array = GetNamedArrayField(call);
            var index = call.Arg(0, "index");
            var value = call.Arg(1, "value");
            return new ExprJsExplicit(ctx, "array.splice(index, 0, value)", ctx.Void, array, index, value);
        }

        public void InsertRange(int index, IEnumerable<T> collection) {
            foreach (var item in collection) {
                this.Insert(index++, item);
            }
        }

        public int IndexOf(T item) {
            return this.IndexOf(item, 0, this.array.Length);
        }

        public int IndexOf(T item, int index) {
            return this.IndexOf(item, index, this.array.Length - index);
        }

        public int IndexOf(T item, int index, int count) {
            var end = index + count;
            var comp = EqualityComparer<T>.Default;
            for (int i = index; i < end; i++) {
                if (comp.Equals(item, this.array[i])) {
                    return i;
                }
            }
            return -1;
        }

        [Js(typeof(void))]
        public static Expr Sort(ICall call) {
            var ctx = call.Ctx;
            var t = call.CallMethod.DeclaringType.GetGenericArgument(0);
            var mGenSort = ((Action<T[]>)Array.Sort<T>).Method.GetGenericMethodDefinition();
            var mSort = ctx.Module.Import(mGenSort).MakeGeneric(t);
            return new ExprCall(ctx, mSort, null, GetNamedArrayField(call).Expr);
        }

        [Js("Sort", typeof(void), typeof(IComparer<GenTypeParam0>))]
        public static Expr SortComparer(ICall call) {
            var ctx = call.Ctx;
            var t = call.CallMethod.DeclaringType.GetGenericArgument(0);
            var tComparer = call.CallMethod.Parameters.First().ParameterType.FullResolve(call.CallMethod);
            var mGenSort = ((Action<T[], IComparer<T>>)Array.Sort<T>).Method.GetGenericMethodDefinition();
            var mSort = ctx.Module.Import(mGenSort).MakeGeneric(t);
            var comparer = call.Arg(0, "comparer");
            var mDefaultComparerNet = typeof(Comparer<T>).GetProperty("Default").GetMethod;
            var mDefaultComparer = ctx.Module.Import(mDefaultComparerNet);
            var defaultComparerCall = new ExprCall(ctx, mDefaultComparer, null).Named("defaultComparerCall");
            var ensureComparer = new ExprJsExplicit(ctx, "(comparer || defaultComparerCall)", tComparer, comparer, defaultComparerCall);
            return new ExprCall(ctx, mSort, null, GetNamedArrayField(call).Expr, ensureComparer);
        }

        [Js("BinarySearch", typeof(int), typeof(GenTypeParam0))]
        public static Expr BinarySearchItem(ICall call) {
            var ctx = call.Ctx;
            var t = call.CallMethod.DeclaringType.GetGenericArgument(0);
            var mGenBinarySearch = ((Func<T[], T, int>)Array.BinarySearch<T>).Method.GetGenericMethodDefinition();
            var mBinarySearch = ctx.Module.Import(mGenBinarySearch).MakeGeneric(t);
            return new ExprCall(ctx, mBinarySearch, null, GetNamedArrayField(call).Expr, call.Arg(0));
        }

        [Js("BinarySearch", typeof(int), typeof(GenTypeParam0), typeof(IComparer<GenTypeParam0>))]
        public static Expr BinarySearchItemComparer(ICall call) {
            var ctx = call.Ctx;
            var t = call.CallMethod.DeclaringType.GetGenericArgument(0);
            var mGenBinarySearch = ((Func<T[], T, IComparer<T>, int>)Array.BinarySearch<T>).Method.GetGenericMethodDefinition();
            var mBinarySearch = ctx.Module.Import(mGenBinarySearch).MakeGeneric(t);
            return new ExprCall(ctx, mBinarySearch, null, GetNamedArrayField(call).Expr, call.Arg(0), call.Arg(1));
        }

        [Js]
        public static Expr ToArray(ICall call) {
            var ctx = call.Ctx;
            var array = GetNamedArrayField(call);
            return new ExprJsExplicit(ctx, "array.slice(0)", call.Type, array);
        }

        void IList<T>.Insert(int index, T item) {
            throw new NotImplementedException();
        }

        [JsRedirect(typeof(List<>))]
        public void RemoveAt(int index) {
            throw new JsImplException();
        }
        [Js]
        public static Expr RemoveAt(ICall call) {
            var ctx = call.Ctx;
            var array = GetNamedArrayField(call);
            var index = call.Arg(0, "index");
            return new ExprJsExplicit(ctx, "array.splice(index, 1)", ctx.Void, array, index);
        }

        [JsRedirect(typeof(List<>))]
        public T this[int index] {
            get { throw new JsImplException(); }
            set { throw new JsImplException(); }
        }
        [Js]
        public static Expr get_Item(ICall call) {
            var ctx = call.Ctx;
            var genType = call.CallMethod.DeclaringType.GetGenericArgument(0);
            var array = GetNamedArrayField(call);
            var index = call.Arg(0, "index");
            return new ExprJsExplicit(ctx, "array[index]", genType, array, index);
        }

        [Js]
        public static Expr set_Item(ICall call) {
            var ctx = call.Ctx;
            var genType = call.CallMethod.DeclaringType.GetGenericArgument(0);
            var array = GetNamedArrayField(call);
            var index = call.Arg(0, "index");
            var value = call.Arg(1, "value");
            return new ExprJsExplicit(ctx, "array[index] = value", genType, array, index, value);
        }

        [JsRedirect(typeof(List<>))]
        public void Add(T item) { throw new JsImplException(); }
        [Js]
        public static Expr Add(ICall call) {
            var ctx = call.Ctx;
            var array = GetNamedArrayField(call);
            var value = call.Arg(0, "value");
            return new ExprJsExplicit(ctx, "array.push(value)", ctx.Void, array, value);
        }

        public void AddRange(IEnumerable<T> collection) {
            foreach (var item in collection) {
                this.Add(item);
            }
        }

        [JsRedirect(typeof(List<>))]
        public void Clear() { throw new JsImplException(); }
        [Js]
        public static Expr Clear(ICall call) {
            var ctx = call.Ctx;
            var array = GetNamedArrayField(call);
            return new ExprJsExplicit(ctx, "array.length = 0", ctx.Void, array);
        }

        public bool Contains(T item) {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        [JsRedirect(typeof(List<>))]
        public int Count {
            get { throw new JsImplException(); }
        }
        [Js]
        public static Expr get_Count(ICall call) {
            var ctx = call.Ctx;
            var array = GetNamedArrayField(call);
            return new ExprJsExplicit(ctx, "array.length", ctx.Int32, array);
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(T item) {
            throw new NotImplementedException();
        }

        [JsDetail(Signature = new[] { typeof(List<GenTypeParam0>.Enumerator) })]
        public Enumerator GetEnumerator() {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new Enumerator(this);
        }

        public int Add(object value) {
            throw new NotImplementedException();
        }

        public bool Contains(object value) {
            throw new NotImplementedException();
        }

        public int IndexOf(object value) {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value) {
            throw new NotImplementedException();
        }

        public bool IsFixedSize {
            get { throw new NotImplementedException(); }
        }

        public void Remove(object value) {
            throw new NotImplementedException();
        }

        object IList.this[int index] {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public void CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        public bool IsSynchronized {
            get { throw new NotImplementedException(); }
        }

        public object SyncRoot {
            get { throw new NotImplementedException(); }
        }
    }

}
