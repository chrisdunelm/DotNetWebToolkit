using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    class VisitorPhiClusters : AstRecursiveVisitor {

        public static IEnumerable<IEnumerable<ExprVar>> V(ICode c) {
            var v = new VisitorPhiClusters();
            v.Visit(c);
            var ret = UniqueClusters(v.clusters);
            return ret;
        }

        public static IEnumerable<ExprVar[]> UniqueClusters(IEnumerable<ExprVar[]> clusters) {
        start:
            foreach (var a in clusters) {
                foreach (var b in clusters.Where(x => a != x)) {
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
            this.clusters.Add(e.Exprs.OfType<ExprVar>().Concat(e).ToArray());
            return e;
        }

    }
}
