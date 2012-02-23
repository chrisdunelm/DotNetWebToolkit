using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Output;
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

    public static class ICallExtensions {

        public static Expr Arg(this ICall call, int argIndex) {
            return call.Args.ElementAt(argIndex);
        }

        public static NamedExpr Arg(this ICall call, int argIndex, string name) {
            return call.Arg(argIndex).Named(name);
        }

    }

}
