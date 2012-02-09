using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Analysis;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Output {
    class VisitorJsPhiClusters : JsAstVisitor {

        public static IEnumerable<IEnumerable<ExprVar>> V(ICode c) {
            var v = new VisitorJsPhiClusters();
            v.Visit(c);
            var ret = VisitorPhiClusters.UniqueClusters(v.clusters);
            return ret;
        }

        private VisitorJsPhiClusters() { }

        private List<ExprVar[]> clusters = new List<ExprVar[]>();

        protected override ICode VisitVarPhi(ExprVarPhi e) {
            this.clusters.Add(e.Exprs.OfType<ExprVar>().Concat(e).ToArray());
            return e;
        }

    }
}
