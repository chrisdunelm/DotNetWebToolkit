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
                var objType = e.Obj.Type.FullResolve(e.Ctx);
                if (objType.Resolve().IsSealed) {
                    // Virtual calls to sealed classes can be rewritten as instance calls
                    var methods = objType.EnumResolvedMethods().ToArray();
                    var instMethod = methods.FirstOrDefault(x => x.MatchMethodOnly(e.CallMethod));
                    if (instMethod != null) {
                        var expr = new ExprCall(e.Ctx, instMethod, e.Obj, e.Args, false);
                        return expr;
                    }
                }
            }
            return base.VisitCall(e);
        }

    }
}
