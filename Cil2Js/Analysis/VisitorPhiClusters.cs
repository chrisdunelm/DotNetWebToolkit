using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    class VisitorPhiClusters : AstRecursiveVisitor {

        public class EqualityComparer : IEqualityComparer<Expr> {

            public EqualityComparer(IEnumerable<IEnumerable<Expr>> phiClusters) {
                this.phiMap = phiClusters
                    .SelectMany(x => x.Select(y => new { key = x.First(), expr = y }))
                    .ToDictionary(x => x.expr, x => x.key);
            }

            private Dictionary<Expr, Expr> phiMap;

            public bool Equals(Expr x, Expr y) {
                if (x.ExprType == Expr.NodeType.VarPhi) {
                    x = ((ExprVarPhi)x).Exprs.First();
                }
                if (y.ExprType == Expr.NodeType.VarPhi) {
                    y = ((ExprVarPhi)y).Exprs.First();
                }
                return this.phiMap.ValueOrDefault(x, x) == this.phiMap.ValueOrDefault(y, y);
            }

            public int GetHashCode(Expr obj) {
                if (obj.ExprType == Expr.NodeType.VarPhi) {
                    obj = ((ExprVarPhi)obj).Exprs.First();
                }
                return this.phiMap.ValueOrDefault(obj, obj).GetHashCode();
            }
        }

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
