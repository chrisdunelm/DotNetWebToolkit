using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil;

namespace Test.Utils {
    public class TestBase {

        private static Ctx ctxCache = null;
        protected static Ctx Ctx {
            get {
                if (ctxCache == null) {
                    var mod = AssemblyDefinition.ReadAssembly(System.Reflection.Assembly.GetExecutingAssembly().Location).MainModule;
                    var curMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    var type = mod.GetType(curMethod.DeclaringType.FullName);
                    var method = type.Methods.First(x => x.Name == curMethod.Name);
                    ctxCache = new Ctx(type, method);
                }
                return ctxCache;
            }
        }

        protected static Expr.Gen ExprGen { get { return Ctx.ExprGen; } }

        protected static ExprLiteral True { get { return new ExprLiteral(Ctx, true, Ctx.Boolean); } }
        protected static ExprLiteral False { get { return new ExprLiteral(Ctx, false, Ctx.Boolean); } }

    }
}
