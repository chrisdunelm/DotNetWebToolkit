using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Utils {

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    abstract class ParamAttribute : Attribute {

        public virtual int? MinIterations {
            get { return null; }
        }

        public virtual bool GenBool(Random rnd, int iteration) {
            throw new NotSupportedException();
        }

        public virtual SByte GenSByte(Random rnd, int iteration) {
            throw new NotSupportedException();
        }

        public virtual Byte GenByte(Random rnd, int iteration) {
            throw new NotSupportedException();
        }

        public virtual Int16 GenInt16(Random rnd, int iteration) {
            throw new NotSupportedException();
        }

        public virtual Int32 GenInt32(Random rnd, int iteration) {
            throw new NotSupportedException();
        }

        public virtual Int64 GenInt64(Random rnd, int iteration) {
            throw new NotSupportedException();
        }

        public virtual UInt16 GenUInt16(Random rnd, int iteration) {
            throw new NotSupportedException();
        }

        public virtual UInt32 GenUInt32(Random rnd, int iteration) {
            throw new NotSupportedException();
        }

        public virtual UInt64 GenUInt64(Random rnd, int iteration) {
            throw new NotSupportedException();
        }

        public virtual Single GenSingle(Random rnd, int iteration) {
            throw new NotSupportedException();
        }

        public virtual Double GenDouble(Random rnd, int iteration) {
            throw new NotSupportedException();
        }

        public virtual string GenString(Random rnd, int iteration) {
            throw new NotSupportedException();
        }

        public virtual char GenChar(Random rnd, int iteration) {
            throw new NotSupportedException();
        }

    }

    class ParamPositiveAttribute : ParamAttribute {

        public override int GenInt32(Random rnd, int iteration) {
            return rnd.Next(1, 101);
        }

    }

    class ParamNonZeroAttribute : ParamAttribute {

        public override int GenInt32(Random rnd, int iteration) {
            var i = rnd.Next(-100, 100);
            if (i >= 0) {
                i++;
            }
            return i;
        }

        public override double GenDouble(Random rnd, int iteration) {
            const double delta = 0.001;
            var d = rnd.NextDouble() * 200.0 - 100.0;
            if (d >= 0 && d < delta) {
                d += delta;
            }
            if (d <= 0 && d > -delta) {
                d -= delta;
            }
            return d;
        }

    }

    class ParamAnyAttribute : ParamAttribute {

        public override int GenInt32(Random rnd, int iteration) {
            return rnd.Next(-100, 101);
        }

        public override double GenDouble(Random rnd, int iteration) {
            return rnd.NextDouble() * 200.0 - 100.0;
        }

    }

    class ParamFullRangeAttribute : ParamAttribute {

        public ParamFullRangeAttribute() {
            this.min = this.max = null;
        }

        public ParamFullRangeAttribute(object min, object max) {
            this.min = min;
            this.max = max;
        }

        private object min, max;

        public override int? MinIterations {
            get { return 15; }
        }

        private T Gen<T>(T[] template, int iteration, Func<T> fnRnd) where T : IComparable<T> {
            T v;
            if (iteration < template.Length) {
                v = template[iteration];
            } else {
                v = fnRnd();
            }
            if (this.min != null && v.CompareTo((T)Convert.ChangeType(this.min, typeof(T))) < 0) { // v < this.min
                v = (T)Convert.ChangeType(this.min, typeof(T));
            }
            if (this.max != null && v.CompareTo((T)Convert.ChangeType(this.max, typeof(T))) > 0) { // v > this.max
                v = (T)Convert.ChangeType(this.max, typeof(T));
            }
            return v;
        }

        private static SByte[] SByteValues = { SByte.MinValue, SByte.MaxValue, 0, 1, -1, SByte.MinValue + 1, SByte.MaxValue - 1 };
        private static Byte[] ByteValues = { 0, 255, 1, 254 };
        private static Int16[] Int16Values = { Int16.MinValue, Int16.MaxValue, 0, 1, -1, Int16.MinValue + 1, Int16.MaxValue - 1 };
        private static Int32[] Int32Values = { int.MinValue, int.MaxValue, 0, 1, -1, int.MinValue + 1, int.MaxValue - 1 };
        private static Int64[] Int64Values = {
                                                 Int64.MinValue, Int64.MaxValue, Int64.MinValue + 1, Int64.MaxValue - 1,
                                                 Int32.MinValue, Int32.MinValue - 1L, Int32.MaxValue, Int32.MaxValue + 1L,
                                                 0, -1, 1, 
                                             };
        private static UInt16[] UInt16Values = { 0, UInt16.MaxValue, 1, UInt16.MaxValue - 1 };
        private static UInt32[] UInt32Values = { 0, UInt32.MaxValue, 1, UInt32.MaxValue - 1 };
        private static UInt64[] UInt64Values = { 
                                                   0, UInt64.MaxValue, 1, UInt64.MaxValue - 1,
                                                   int.MaxValue, int.MaxValue + 1L, uint.MaxValue, uint.MaxValue + 1L
                                               };
        private static Single[] SingleValues = {
                                                   Single.NaN, Single.NegativeInfinity, Single.PositiveInfinity,
                                                   Single.MaxValue, -Single.MaxValue, Single.MinValue, -Single.MinValue,
                                                   0.0f, -1.0f, 1.0f
                                               };
        private static Double[] DoubleValues = {
                                                   Double.NaN, Double.NegativeInfinity, Double.PositiveInfinity,
                                                   Double.MaxValue, -Double.MaxValue, Double.MinValue, -Double.MinValue,
                                                   0.0, -1.0, 1.0
                                               };

        private static byte[] Bc(Random rnd, int size) {
            var b = new byte[size];
            rnd.NextBytes(b);
            return b;
        }

        public override sbyte GenSByte(Random rnd, int iteration) {
            return this.Gen(SByteValues, iteration, () => (sbyte)rnd.Next(0, 256));
        }

        public override byte GenByte(Random rnd, int iteration) {
            return this.Gen(ByteValues, iteration, () => (byte)rnd.Next(0, 256));
        }

        public override short GenInt16(Random rnd, int iteration) {
            return this.Gen(Int16Values, iteration, () => (short)rnd.Next(short.MinValue + 2, short.MaxValue - 1));
        }

        public override int GenInt32(Random rnd, int iteration) {
            return this.Gen(Int32Values, iteration, () => rnd.Next(int.MinValue + 2, int.MaxValue - 1));
        }

        public override long GenInt64(Random rnd, int iteration) {
            return this.Gen(Int64Values, iteration, () => BitConverter.ToInt64(Bc(rnd, 8), 0));
        }

        public override ushort GenUInt16(Random rnd, int iteration) {
            return this.Gen(UInt16Values, iteration, () => BitConverter.ToUInt16(Bc(rnd, 2), 0));
        }

        public override uint GenUInt32(Random rnd, int iteration) {
            return this.Gen(UInt32Values, iteration, () => BitConverter.ToUInt32(Bc(rnd, 4), 0));
        }

        public override ulong GenUInt64(Random rnd, int iteration) {
            return this.Gen(UInt64Values, iteration, () => BitConverter.ToUInt64(Bc(rnd, 8), 0));
        }

        public override float GenSingle(Random rnd, int iteration) {
            return this.Gen(SingleValues, iteration, () => (float)(rnd.NextDouble() - 0.5) * float.MaxValue);
        }

        public override double GenDouble(Random rnd, int iteration) {
            return this.Gen(DoubleValues, iteration, () => (rnd.NextDouble() - 0.5) * double.MaxValue);
        }

    }

    class ParamFullRangeNonZeroAttribute : ParamFullRangeAttribute {

        public override long GenInt64(Random rnd, int iteration) {
            var v = base.GenInt64(rnd, iteration);
            if (v == 0) {
                v = 1;
            }
            return v;
        }

        public override ulong GenUInt64(Random rnd, int iteration) {
            var v = base.GenUInt64(rnd, iteration);
            if (v == 0) {
                v = 1;
            }
            return v;
        }

    }

}
