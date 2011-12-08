using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Ast {
    public interface ICall : ICode {

        MethodReference Calling { get; }
        IEnumerable<Expr> Args { get; }
        TypeReference Type { get; }

    }
}
