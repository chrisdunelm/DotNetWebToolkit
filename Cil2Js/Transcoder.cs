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
using System.IO;

namespace DotNetWebToolkit.Cil2Js {
    public static class Transcoder {

        public static ICode ToAst(MethodReference mRef, TypeReference tRef, bool verbose = false) {
            var ctx = new Ctx(tRef, mRef);
            return ToAst(ctx, verbose);
        }

        internal static void Print(Stmt stmt, string name, bool verbose) {
            if (verbose) {
                if (name != null && name.StartsWith("Visitor")) {
                    name = name.Substring(7);
                }
                Console.WriteLine(" --- AST Transform Step {0}{1} ---", stmt.Ctx.step++, name == null ? "" : (" '" + name + "'"));
                Console.WriteLine(ShowVisitor.V(stmt));
                Console.WriteLine();
            }
        }

        internal static Stmt DoStep(Func<Stmt, Stmt> fnStep, Stmt stmt, string name, bool verbose) {
            var s1 = fnStep(stmt);
            if (s1 != stmt) {
                Print(s1, name, verbose);
                var dupStmts = VisitorFindDuplicateStmts.Find(s1);
                if (dupStmts.Any()) {
                    Console.WriteLine("*** ERROR *** {0} DUPLICATE STMT(S) ***", dupStmts.Count());
                    foreach (var dup in dupStmts) {
                        Console.WriteLine();
                        Console.WriteLine(ShowVisitor.V(dup));
                    }
                    throw new InvalidOperationException("Duplicate stmt(s) found");
                }
            }
            return s1;
        }

        public static ICode ToAst(Ctx ctx, bool verbose = false) {
            var ast = AstGenerator.CreateBlockedCilAst(ctx);
            Print(ast, null, verbose);
            ast = DoStep(s => (Stmt)VisitorConvertCilToSsa.V(s), ast, "VisitorConvertCilToSsa", verbose);
            // Reduce to AST with no continuations
            for (int i = 0; ; i++) {
                var astOrg = ast;
                ast = DoStep(s => (Stmt)VisitorSubstitute.V(s), ast, "VisitorSubstitute", verbose);
                ast = DoStep(s => (Stmt)VisitorTryCatchFinallySequencing.V(s), ast, "VisitorTryCatchFinallySequencing", verbose);
                ast = DoStep(s => (Stmt)VisitorIfDistribution.V(s), ast, "VisitorIfDistribution", verbose);
                ast = DoStep(s => (Stmt)VisitorDerecurse.V(s), ast, "VisitorDerecurse", verbose);
                ast = DoStep(s => (Stmt)VisitorBooleanSimplification.V(s), ast, "VisitorBooleanSimplification", verbose);
                ast = DoStep(s => (Stmt)VisitorIfSimplification.V(s), ast, "VisitorIfSimplification", verbose);
                ast = DoStep(s => (Stmt)VisitorIfReorder.V(s), ast, "VisitorIfReorder", verbose);
                ast = DoStep(s => (Stmt)VisitorEmptyBlockRemoval.V(s), ast, "VisitorEmptyBlockRemoval", verbose);
                ast = DoStep(s => (Stmt)VisitorSwitchSequencing.V(s, false), ast, "VisitorSwitchSequencing", verbose);
                if (ast == astOrg) {
                    if (!VisitorFindContinuations.Any(ast)) {
                        break;
                    } else {
                        ast = DoStep(s => (Stmt)VisitorSwitchSequencing.V(s, true), ast, "VisitorSwitchSequencing-LastChance", verbose);
                        if (ast != astOrg) {
                            continue;
                        }
                        ast = DoStep(s => (Stmt)VisitorSubstituteIrreducable.V(s), ast, "VisitorSubstituteIrreducable", verbose);
                        if (ast != astOrg) {
                            continue;
                        }
                        ast = DoStep(s => (Stmt)VisitorDuplicateCode.V(s), ast, "VisitorDuplicateCode", verbose);
                        if (ast != astOrg) {
                            continue;
                        }
                        throw new InvalidOperationException("Error: Cannot reduce IL to AST with no continuations");
                    }
                }
                if (i > 20) {
                    // After 20 iterations even the most complex method should be sorted out
                    throw new InvalidOperationException("Error: Stuck in loop trying to reduce AST");
                }
            }
            // PhiSimplifier must be done before DefiniteAssignment
            ast = DoStep(s => (Stmt)VisitorPhiSimplifier.V(s), ast, "VisitorPhiSimplifier", verbose);
            // DefiniteAssignment must be done before SsaCopyPropagation
            ast = DoStep(s => (Stmt)VisitorDefiniteAssignment.V(s), ast, "VisitorDefiniteAssignment", verbose);
            // Simplify AST
            for (int i = 0; ; i++) {
                var astOrg = ast;
                // TODO: VisitorBooleanSimplification may re-order logic, which should not be done at this stage
                ast = DoStep(s => (Stmt)VisitorBooleanSimplification.V(s), ast, "VisitorBooleanSimplification", verbose);
                ast = DoStep(s => (Stmt)VisitorIfSimplification.V(s), ast, "VisitorIfSimplification", verbose);
                ast = DoStep(s => (Stmt)VisitorMoveOutOfLoop.V(s), ast, "VisitorMoveOutOfLoop", verbose);
                ast = DoStep(s => (Stmt)VisitorSsaCopyPropagation.V(s), ast, "VisitorSsaCopyPropagation", verbose);
                ast = DoStep(s => (Stmt)VisitorPhiSimplifier.V(s), ast, "VisitorPhiSimplifier", verbose);
                ast = DoStep(s => (Stmt)VisitorExpressionSimplifier.V(s), ast, "VisitorExpressionSimplifier", verbose);
                ast = DoStep(s => (Stmt)VisitorEmptyBlockRemoval.V(s), ast, "VisitorEmptyBlockRemoval", verbose);
                if (ast == astOrg) {
                    break;
                }
                if (i > 20) {
                    // After 20 iterations even the most complex method should be sorted out
                    throw new InvalidOperationException("Error: Stuck in loop trying to simplify AST");
                }
            }
            ast = DoStep(s => (Stmt)VisitorRemoveCasts.V(s), ast, "VisitorRemoveCasts", verbose);
            ast = DoStep(s => (Stmt)VisitorRemoveFinalReturn.V(s), ast, "VisitorRemoveFinalReturn", verbose);
            ast = DoStep(s => (Stmt)VisitorTypeCorrector.V(s), ast, "VisitorTypeCorrector", verbose);
            return ast;

        }

        public static MethodReference GetMethod(MethodInfo mi) {
            var filename = mi.DeclaringType.Assembly.Location;
            var module = ModuleDefinition.ReadModule(filename, AssemblyResolvers.ReaderParameters);
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
            //CecilExtensions.SourceDirectory = Path.GetDirectoryName(filename);
            AssemblyResolvers.AddDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            AssemblyResolvers.AddDirectory(Path.GetDirectoryName(filename));
            AssemblyResolvers.ReaderParameters.ReadSymbols = true;
            var module = ModuleDefinition.ReadModule(filename, AssemblyResolvers.ReaderParameters);
            return ToJs(module, verbose);
        }

        public static string ToJs(ModuleDefinition module, bool verbose = false) {
            Func<TypeDefinition, IEnumerable<TypeDefinition>> getSelfAndNestedTypes = null;
            getSelfAndNestedTypes = type => type.NestedTypes.SelectMany(x => getSelfAndNestedTypes(x)).Concat(type);
            var exportedTypes = module.Types
                .SelectMany(x => getSelfAndNestedTypes(x))
                .Where(x => x.GetCustomAttribute<JsExportAttribute>() != null)
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
