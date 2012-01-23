using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;

namespace DotNetWebToolkit.Cil2Js.Output {
    class VisitorJsReplace : JsAstVisitor {

        public static ICode V(ICode ast, ICode find, ICode replace) {
            var v = new VisitorJsReplace(find, replace);
            return v.Visit(ast);
        }

        public VisitorJsReplace(ICode find, ICode replace) {
            this.find = find;
            this.replace = replace;
        }

        private ICode find, replace;

        public override ICode Visit(ICode c) {
            if (c == this.find) {
                return this.replace;
            }
            return base.Visit(c);
        }

    }
}
