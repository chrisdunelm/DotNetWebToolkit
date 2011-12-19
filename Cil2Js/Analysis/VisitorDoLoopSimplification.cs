using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Cil2Js.Ast;

namespace Cil2Js.Analysis {
    public class VisitorDoLoopSimplification : AstRecursiveVisitor {

        public static ICode V(MethodDefinition method, ICode c) {
            var v = new VisitorDoLoopSimplification(method);
            return v.Visit(c);
        }

        private VisitorDoLoopSimplification(MethodDefinition method) {
            this.typeSystem = method.Module.TypeSystem;
        }

        private TypeSystem typeSystem;

        protected override ICode VisitDoLoop(StmtDoLoop s) {
            var body = (Stmt)this.Visit(s.Body);
            if (body == null) {
                // Loop has no body
                return null;
            }
            if (s.While.IsLiteralBoolean(false)) {
                // Will never loop
                return body;
            }
            if (body != s.Body) {
                return new StmtDoLoop(s.Ctx, body, s.While);
            } else {
                return s;
            }
        }

        protected override ICode VisitBlock(StmtBlock s) {
            bool restGone = false;
            // Remove all code after an infinite loop
            return this.HandleBlock(s, stmt => {
                if (restGone) {
                    return null;
                }
                if (stmt.StmtType == Stmt.NodeType.DoLoop) {
                    var sDo = (StmtDoLoop)stmt;
                    if (sDo.While.IsLiteralBoolean(true)) {
                        restGone = true;
                    }
                }
                return stmt;
            });
        }

    }
}
