using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Cil2Js.Utils;

namespace Cil2Js.Analysis {
    class VisitorPhiClusters : AstRecursiveVisitor {

        public static IEnumerable<IEnumerable<ExprVar>> V(ICode c) {
            var v = new VisitorPhiClusters();
            v.Visit(c);
            var ret = UniqueClusters(v.clusters);
            return ret;
        }

        private static IEnumerable<ExprVar[]> UniqueClusters(IEnumerable<ExprVar[]> clusters) {
        start:
            foreach (var a in clusters) {
                foreach (var b in clusters) {
                    if (a == b) {
                        continue;
                    }
                    if (a.Intersect(b).Any()) {
                        clusters = clusters.Where(x => x != a && x != b).Concat(a.Union(b).ToArray()).ToArray();
                        goto start;
                    }
                }
            }
            return clusters;
        }

        private VisitorPhiClusters() { }

        private List<ExprVar[]> clusters = new List<ExprVar[]>();

        protected override ICode VisitVarPhi(ExprVarPhi e) {
            this.clusters.Add(e.Exprs.Where(x => x.ExprType == Expr.NodeType.VarLocal).Cast<ExprVar>().ToArray());
            return e;
        }


    }
}
