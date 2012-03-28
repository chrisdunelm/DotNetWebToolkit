using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Attributes;

#pragma warning disable 0626, 0824

namespace DotNetWebToolkit.Web {

    [JsClass("Event")] // TODO: check name
    public class Event {

        private Event() { }

        public extern string Type { get; }
        //public extern EventTarget Target { get; }
        //public extern EventTarget CurrentTarget { get; }
        public extern EventPhase EventPhase { get; }
        public extern bool Bubbles { get; }
        public extern bool Cancelable { get; }
        //public extern DateTime TimeStamp { get; }

        public extern void StopPropagation();
        public extern void PreventDefault();

    }

    public enum EventPhase {
        CapturingPhase = 1,
        AtTarget = 2,
        BubblingPhase = 3,
    }

}
