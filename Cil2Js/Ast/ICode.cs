using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cil2Js.Ast {

    public enum CodeType {
        Expression,
        Statement,
    }

    public interface ICode : ICloneable {

        CodeType CodeType { get; }

    }
}
