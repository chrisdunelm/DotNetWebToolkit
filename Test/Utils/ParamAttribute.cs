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

        public virtual int GenInt32(Random rnd, int iteration) {
            throw new NotSupportedException();
        }

        public virtual double GenDouble(Random rnd, int iteration) {
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

        public override int? MinIterations {
            get { return 10; }
        }

        public override int GenInt32(Random rnd, int iteration) {
            switch (iteration) {
            case 0: return int.MinValue;
            case 1: return int.MaxValue;
            case 2: return 0;
            case 3: return 1;
            case 4: return -1;
            case 5: return int.MinValue + 1;
            case 6: return int.MaxValue - 1;
            default: return rnd.Next(int.MinValue + 2, int.MaxValue - 1);
            }
        }

    }

}
