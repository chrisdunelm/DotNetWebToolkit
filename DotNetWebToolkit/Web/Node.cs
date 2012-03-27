using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Attributes;

#pragma warning disable 0626, 0824

namespace DotNetWebToolkit.Web {

    [JsAbstractClass]
    public abstract class Node {

        public extern T AppendChild<T>(T child) where T : Node;

    }

}
