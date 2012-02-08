using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Attributes;

#pragma warning disable 0626

namespace DotNetWebToolkit.Web {

    [JsUseType(typeof(int))]
    public struct IntervalId { }

    [JsUseType(typeof(int))]
    public struct TimeoutId { }

    [JsClass("DOMWindow")]
    public static class Window {

        public static extern int InnerWidth { get; }
        public static extern int InnerHeight { get; }

        public static extern IntervalId SetInterval(Action func, int intervalMs);

        public static IntervalId SetInterval(Action func, TimeSpan interval) {
            return SetInterval(func, (int)interval.TotalMilliseconds);
        }

        public static extern void ClearInterval(IntervalId intervalId);

        public static extern TimeoutId SetTimeout(Action func, int intervalMs);

        public static TimeoutId SetTimeout(Action func, TimeSpan interval) {
            return SetTimeout(func, (int)interval.TotalMilliseconds);
        }

        public static extern void ClearTimeout(TimeoutId timeoutId);

    }

}
