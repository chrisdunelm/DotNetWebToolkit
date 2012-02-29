using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    public class VisitorCounter : AstRecursiveVisitor {

        public static int GetCount(ICode ast, ICode toCount) {
            var v = new VisitorCounter {
                toCount = toCount
            };
            v.Visit(ast);
            return v.count;
        }

        private ICode toCount;
        private int count = 0;

        public override ICode Visit(ICode c) {
            if (c == this.toCount) {
                this.count++;
            }
            return base.Visit(c);
        }

    }
}
