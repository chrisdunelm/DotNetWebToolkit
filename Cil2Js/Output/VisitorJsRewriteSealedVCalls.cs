using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class VisitorJsRewriteSealedVCalls : JsAstVisitor {

        public static ICode V(ICode ast) {
            var v = new VisitorJsRewriteSealedVCalls();
            return v.Visit(ast);
        }

        protected override ICode VisitCall(ExprCall e) {
            if (e.IsVirtualCall) {
                var objType = e.Obj.Type.FullResolve(e.CallMethod);
                if (objType.Resolve().IsSealed) {
                    // Virtual calls to sealed classes can be rewritten as instance calls
                    var instMethod = objType.EnumResolvedMethods().First(x => x.MatchMethodOnly(e.CallMethod));
                    var expr = new ExprCall(e.Ctx, instMethod, e.Obj, e.Args, false);
                    return expr;
                }
            }
            return base.VisitCall(e);
        }

    }
}
