using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public abstract class ExprVar : Expr {

        public ExprVar(Ctx ctx) : base(ctx) { }

    }
}
