using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Utils {

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    abstract class ParamAttribute : Attribute {

        public virtual bool GenBool(Random rnd) {
            throw new NotSupportedException();
        }

        public virtual int GenInt32(Random rnd) {
            throw new NotSupportedException();
        }

        public virtual double GenDouble(Random rnd) {
            throw new NotSupportedException();
        }

        public virtual string GenString(Random rnd) {
            throw new NotSupportedException();
        }

    }

    class ParamPositiveAttribute : ParamAttribute {

        public override int GenInt32(Random rnd) {
            return rnd.Next(1, 101);
        }

    }

    class ParamNonZeroAttribute : ParamAttribute {

        public override int GenInt32(Random rnd) {
            var i = rnd.Next(-100, 100);
            if (i >= 0) {
                i++;
            }
            return i;
        }

        public override double GenDouble(Random rnd) {
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

        public override int GenInt32(Random rnd) {
            return rnd.Next(-100, 101);
        }

        public override double GenDouble(Random rnd) {
            return rnd.NextDouble() * 200.0 - 100.0;
        }

    }

}
