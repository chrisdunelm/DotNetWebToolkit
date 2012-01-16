using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public interface ICall : ICode {

        Expr.NodeType ExprType { get; }
        MethodReference CallMethod { get; }
        bool IsVirtualCall { get; }
        Expr Obj { get; }
        IEnumerable<Expr> Args { get; }
        TypeReference Type { get; }

    }
}
