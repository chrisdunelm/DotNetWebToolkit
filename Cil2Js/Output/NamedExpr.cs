using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;

namespace DotNetWebToolkit.Cil2Js.Output {

    public class NamedExpr {

        public NamedExpr(Expr expr, string name) {
            this.Expr = expr;
            this.Name = name;
        }

        public Expr Expr { get; private set; }
        public string Name { get; private set; }

        public override string ToString() {
            return string.Format("{{ \"{0}\": {1} }}", this.Name, this.Expr);
        }

    }

    static class NamedExprExtensions {

        public static NamedExpr Named(this Expr expr, string name) {
            return new NamedExpr(expr, name);
        }

    }

}
