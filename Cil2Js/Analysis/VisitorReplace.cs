using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;

namespace Cil2Js.Analysis {
    public class VisitorReplace : AstRecursiveVisitor {

        public static ICode V(ICode c, ICode find, ICode replace) {
            var v = new VisitorReplace(find, replace);
            return v.Visit(c);
        }

        public VisitorReplace(ICode find, ICode replace) {
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
