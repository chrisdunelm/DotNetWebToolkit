using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Output {
    class VisitorJsResolveDelegates : JsAstVisitor {

        public static ICode V(ICode ast) {
            var v = new VisitorJsResolveDelegates();
            return v.Visit(ast);
        }

        protected override ICode VisitNewObj(ExprNewObj e) {
            var isDelegate = e.CallMethod.DeclaringType.EnumThisAllBaseTypes().Any(x => x.IsDelegate());
            if (isDelegate) {
                var ctx = e.Ctx;
                var mRef = ((ExprMethodReference)e.Args.ElementAt(1)).Method;
                var obj = mRef.HasThis ? e.Args.ElementAt(0) : null;
                return new ExprJsDelegateCtor(ctx, e.Type, obj, mRef);
            } else {
                return base.VisitNewObj(e);
            }
        }

        protected override ICode VisitCall(ExprCall e) {
            var isDelegate = e.CallMethod.DeclaringType.EnumThisAllBaseTypes().Any(x => x.IsDelegate());
            if (isDelegate) {
                var ctx = e.Ctx;
                switch (e.CallMethod.Name) {
                case "Invoke":
                    return new ExprJsDelegateInvoke(ctx, e.Obj, e.Args);
                default:
                    throw new NotSupportedException("Cannot handle delegate call: " + e.CallMethod.Name);
                }
            } else {
                return base.VisitCall(e);
            }
        }

    }
}
