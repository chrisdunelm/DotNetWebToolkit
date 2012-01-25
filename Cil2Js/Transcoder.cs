using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Analysis;
using DotNetWebToolkit.Cil2Js.Output;
using System.Reflection;
using DotNetWebToolkit.Cil2Js.Utils;
using DotNetWebToolkit.Attributes;

namespace DotNetWebToolkit.Cil2Js {
    public static class Transcoder {

        public static ICode ToAst(MethodReference mRef, TypeReference tRef, bool verbose = false) {
            var ctx = new Ctx(tRef, mRef);
            return ToAst(ctx, verbose);
        }

        public static ICode ToAst(Ctx ctx, bool verbose = false) {
            var ast = AstGenerator.CreateBlockedCilAst(ctx);
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
            print(ast, null);
            ast = doStep(s => (Stmt)VisitorConvertCilToSsa.V(s), ast, "VisitorConvertCilToSsa");
            // Reduce to AST with no continuations
            HashSet<Expr> booleanSimplification = new HashSet<Expr>();
            for (int i = 0; ; i++) {
                var astOrg = ast;
                ast = doStep(s => (Stmt)VisitorSubstitute.V(s), ast, "VisitorSubstitute");
                ast = doStep(s => (Stmt)VisitorTryCatchFinallySequencing.V(s), ast, "VisitorTryCatchFinallySequencing");
                ast = doStep(s => (Stmt)VisitorIfDistribution.V(s), ast, "VisitorIfDistribution");
                ast = doStep(s => (Stmt)VisitorDerecurse.V(s), ast, "VisitorDerecurse");
                ast = doStep(s => (Stmt)VisitorBooleanSimplification.V(s, booleanSimplification, out booleanSimplification), ast, "VisitorBooleanSimplification");
                ast = doStep(s => (Stmt)VisitorIfSimplification.V(s), ast, "VisitorIfSimplification");
                ast = doStep(s => (Stmt)VisitorIfReorder.V(s), ast, "VisitorIfReorder");
                ast = doStep(s => (Stmt)VisitorConditionRemoval.V(s), ast, "VisitorConditionRemoval");
                ast = doStep(s => (Stmt)VisitorEmptyBlockRemoval.V(s), ast, "VisitorEmptyBlockRemoval");
                ast = doStep(s => (Stmt)VisitorSwitchSequencing.V(s), ast, "VisitorSwitchSequencing");
                if (ast == astOrg) {
                    break;
                }
                if (i > 20) {
                    // After 20 iterations even the most complex method should be sorted out
                    throw new InvalidOperationException("Error: Stuck in loop trying to reduce AST");
                }
            }
            if (VisitorFindContinuations.Any(ast)) {
                throw new InvalidOperationException("Error: Cannot reduce IL to AST with no continuations");
            }
            // Simplify AST
            for (int i = 0; ; i++) {
                var astOrg = ast;
                // TODO: VisitorBooleanSimplification may re-order logic, which should not be done at this stage
                ast = doStep(s => (Stmt)VisitorBooleanSimplification.V(s, booleanSimplification, out booleanSimplification), ast, "VisitorBooleanSimplification");
                ast = doStep(s => (Stmt)VisitorIfSimplification.V(s), ast, "VisitorIfSimplification");
                ast = doStep(s => (Stmt)VisitorMoveOutOfLoop.V(s), ast, "VisitorMoveOutOfLoop");
                //ast = doStep(s => (Stmt)VisitorConditionRemoval.V(s), ast, "VisitorConditionRemoval");
                ast = doStep(s => (Stmt)VisitorSsaSimplifier.V(s), ast, "VisitorSsaSimplifier");
                ast = doStep(s => (Stmt)VisitorPhiSimplifier.V(s), ast, "VisitorPhiSimplifier");
                ast = doStep(s => (Stmt)VisitorExpressionSimplifier.V(s), ast, "VisitorExpressionSimplifier");
                ast = doStep(s => (Stmt)VisitorEmptyBlockRemoval.V(s), ast, "VisitorEmptyBlockRemoval");
                if (ast == astOrg) {
                    break;
                }
                if (i > 20) {
                    // After 20 iterations even the most complex method should be sorted out
                    throw new InvalidOperationException("Error: Stuck in loop trying to simplify AST");
                }
            }
            ast = doStep(s => (Stmt)VisitorRemoveCasts.V(s), ast, "VisitorRemoveCasts");
            ast = doStep(s => (Stmt)VisitorRemoveFinalReturn.V(s), ast, "VisitorRemoveFinalReturn");
            ast = doStep(s => (Stmt)VisitorTypeCorrector.V(s), ast, "VisitorTypeCorrector");
            return ast;

        }

        public static MethodReference GetMethod(MethodInfo mi) {
            var filename = mi.DeclaringType.Assembly.Location;
            var module = ModuleDefinition.ReadModule(filename);
            var method = module.Import(mi);
            return method;
        }

        public static string ToJs(MethodReference method, bool verbose = false) {
            return Js.CreateFrom(method, verbose);
        }

        public static string ToJs(MethodInfo methodInfo, bool verbose = false) {
            return Js.CreateFrom(GetMethod(methodInfo), verbose);
        }

        public static string ToJs(string filename, bool verbose = false) {
            var module = ModuleDefinition.ReadModule(filename);
            return ToJs(module, verbose);
        }

        public static string ToJs(ModuleDefinition module, bool verbose = false) {
            Func<TypeDefinition, IEnumerable<TypeDefinition>> getSelfAndNestedTypes = null;
            getSelfAndNestedTypes = type => type.NestedTypes.SelectMany(x => getSelfAndNestedTypes(x)).Concat(type);
            var exportedTypes = module.Types
                .SelectMany(x => getSelfAndNestedTypes(x))
                .Where(x => x.GetCustomAttribute<ExportAttribute>() != null)
                .ToArray();
            if (!exportedTypes.Any()) {
                throw new InvalidOperationException("No types exported");
            }
            var exportedMethods = exportedTypes.SelectMany(x => x.Methods.Where(m => m.IsPublic)).ToArray();
            var nestedTypes = exportedTypes.Where(x => x.IsNested).Select(x => "Nested type: " + x.FullName).ToArray();
            var privateTypes = exportedTypes.Where(x => !(x.IsPublic || x.IsNestedPublic)).Select(x => "Private type: " + x.FullName).ToArray();
            var genTypes = exportedTypes.Where(x => x.HasGenericParameters).Select(x => "Generic type: " + x.FullName).ToArray();
            var genMethods = exportedMethods.Where(x => x.HasGenericParameters).Select(x => "Generic method: " + x.FullName).ToArray();
            var errors = nestedTypes.Concat(privateTypes).Concat(genTypes).Concat(genMethods).ToArray();
            if (errors.Any()) {
                var msg = string.Format("Export errors:{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, errors));
                throw new InvalidOperationException(msg);
            }
            var js = Js.CreateFrom(exportedMethods, verbose);
            return js;
        }

    }
}
