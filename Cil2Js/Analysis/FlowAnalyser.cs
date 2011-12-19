using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil.Cil;
using Mono.Cecil;
using Cil2Js.Utils;
using System.Diagnostics;

namespace Cil2Js.Analysis {
    public static class FlowAnalyser {

        public static Stmt Analyse(Stmt stmt0, bool verbose = false) {
            int step = 0;
            Action<Stmt, string> print = (s, name) => {
                if (verbose) {
                    if (name != null && name.StartsWith("Visitor")) {
                        name = name.Substring(7);
                    }
                    Console.WriteLine(" --- AST Transform Step {0}{1} ---", step++, name == null ? "" : (" '" + name + "'"));
                    Console.WriteLine(ShowVisitor.V(s));
                    Console.WriteLine();
                }
            };
            Func<Func<Stmt, Stmt>, Stmt, string, Stmt> doStep = (fn, s0, name) => {
                var s1 = fn(s0);
                if (s1 != s0) {
                    print(s1, name);
                }
                return s1;
            };
            print(stmt0, null);
            var stmt = doStep(s => (Stmt)VisitorConvertCilToSsa.V(s), stmt0, "VisitorConvertCilToSsa");
            // Reduce to AST with no continuations
            for (int i = 0; ; i++) {
                var stmtOrg = stmt;
                stmt = doStep(s => (Stmt)VisitorSubstitute.V(s), stmt, "VisitorSubstitute");
                stmt = doStep(s => (Stmt)VisitorTryCatchFinallySequencing.V(s), stmt, "VisitorTryCatchFinallySequencing");
                stmt = doStep(s => (Stmt)VisitorIfDistribution.V(s), stmt, "VisitorIfDistribution");
                stmt = doStep(s => (Stmt)VisitorDerecurse.V(s), stmt, "VisitorDerecurse");
                stmt = doStep(s => (Stmt)VisitorBooleanSimplification.V(s), stmt, "VisitorBooleanSimplification");
                stmt = doStep(s => (Stmt)VisitorIfSimplification.V(s), stmt, "VisitorIfSimplification");
                stmt = doStep(s => (Stmt)VisitorConditionRemoval.V(s), stmt, "VisitorConditionRemoval");
                stmt = doStep(s => (Stmt)VisitorEmptyBlockRemoval.V(s), stmt, "VisitorEmptyBlockRemoval");
                if (stmt == stmtOrg) {
                    break;
                }
                if (i > 20) {
                    // After 20 iterations even the most complex method should be sorted out
                    throw new InvalidOperationException("Error: Stuck in loop trying to reduce AST");
                }
            }
            if (VisitorFindContinuations.Any(stmt)) {
                throw new InvalidOperationException("Error: Cannot reduce IL to AST with no continuations");
            }
            // Simplify AST
            for (int i = 0; ; i++) {
                var stmtOrg = stmt;
                stmt = doStep(s => (Stmt)VisitorBooleanSimplification.V(s), stmt, "VisitorBooleanSimplification");
                stmt = doStep(s => (Stmt)VisitorIfSimplification.V(s), stmt, "VisitorIfSimplification");
                stmt = doStep(s => (Stmt)VisitorMoveOutOfLoop.V(s), stmt, "VisitorMoveOutOfLoop");
                stmt = doStep(s => (Stmt)VisitorConditionRemoval.V(s), stmt, "VisitorConditionRemoval");
                stmt = doStep(s => (Stmt)VisitorSsaSimplifier.V(s), stmt, "VisitorSsaSimplifier");
                stmt = doStep(s => (Stmt)VisitorPhiSimplifier.V(s), stmt, "VisitorPhiSimplifier");
                stmt = doStep(s => (Stmt)VisitorExpressionSimplifier.V(s), stmt, "VisitorExpressionSimplifier");
                stmt = doStep(s => (Stmt)VisitorEmptyBlockRemoval.V(s), stmt, "VisitorEmptyBlockRemoval");
                if (stmt == stmtOrg) {
                    break;
                }
                if (i > 20) {
                    // After 20 iterations even the most complex method should be sorted out
                    throw new InvalidOperationException("Error: Stuck in loop trying to simplify AST");
                }
            }
            stmt = doStep(s => (Stmt)VisitorRemoveCasts.V(s), stmt, "VisitorRemoveCasts");
            return stmt;
        }

    }
}
