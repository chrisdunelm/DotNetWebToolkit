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

        public extern bool Bubbles { get; }


    }
}
