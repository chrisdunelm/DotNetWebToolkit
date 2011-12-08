using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Analysis;
using Cil2Js.Ast;

namespace Test.Utils {
    class VisitorCreatePerms : AstVisitor {

        public static IEnumerable<Expr> V(Expr e) {
            var v = new VisitorCreatePerms(e);
            v.Visit(e);
            return v.AllPerms;
        }

        public VisitorCreatePerms(Expr root) {
            this.root = root;
            this.allPerms = new HashSet<Expr> { root };
        }

        private Expr root;
        // Has deepest expr's first
        private List<Tuple<Expr, Expr>> allVariants = new List<Tuple<Expr, Expr>>();
        private HashSet<Expr> allPerms = null;
        public IEnumerable<Expr> AllPerms {
            get {
                return this.allPerms.ToArray();
            }
        }

        protected override ICode VisitBinary(ExprBinary e) {
            switch (e.Op) {
            case BinaryOp.And:
            case BinaryOp.Or:
                var leftPerms = VisitorCreatePerms.V(e.Left);
                var rightPerms = VisitorCreatePerms.V(e.Right);
                foreach (var l in leftPerms) {
                    foreach (var r in rightPerms) {
                        foreach (var t in new[] { Tuple.Create(l, r), Tuple.Create(r, l) }) {
                            Expr perm;
                            if (t.Item1 == e.Left && t.Item2 == e.Right) {
                                perm = e;
                            } else {
                                perm = new ExprBinary(e.Op, e.Type, t.Item1, t.Item2);
                            }
                            this.allPerms.Add((Expr)VisitorReplace.V(this.root, e, perm));
                        }
                    }
                }
                break;
            }
            return e;
        }

    }
}
