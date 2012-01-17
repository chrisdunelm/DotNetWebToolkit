using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Analysis;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class JsMethod : JsAstVisitor {

        public class Resolver {
            public Dictionary<ExprVar, string> LocalVarNames { get; set; }
            public Dictionary<MethodReference, string> MethodNames { get; set; }
            public Dictionary<FieldReference, string> FieldNames { get; set; }
            public Dictionary<TypeReference, string> TypeNames { get; set; }
            public Dictionary<MethodReference, int> VirtualCallIndices { get; set; }
            public Dictionary<MethodReference, int> InterfaceCallIndices { get; set; }
            public Dictionary<TypeReference, string> InterfaceNames { get; set; }
            public Dictionary<TypeData, string> TypeDataNames { get; set; }
        }

        private const int tabSize = 4;

        public static string Create(MethodReference mRef, Resolver resolver, ICode ast) {
            var mDef = mRef.Resolve();
            if (mDef.IsAbstract) {
                throw new ArgumentException("Should never need to transcode an abstract method");
            }
            //if (mDef.IsInternalCall) {
            //    throw new ArgumentException("Cannot transcode an internal method");
            //}
            var tRef = mRef.DeclaringType;
            var tDef = tRef.Resolve();

            var v = new JsMethod(mDef, resolver);
            v.Visit(ast);
            var js = v.js.ToString();

            var sb = new StringBuilder();
            // Method declaration
            var methodName = resolver.MethodNames[mRef];
            var pNames = mRef.Parameters.Select(x => v.parameters.ValueOrDefault(x).NullThru(y => resolver.LocalVarNames[y], "_")).ToArray();
            if (!mDef.IsStatic) {
                var thisName = v.vars.FirstOrDefault(x => x.ExprType == Expr.NodeType.VarThis).NullThru(x => resolver.LocalVarNames[x], "_");
                pNames = pNames.Prepend(thisName).ToArray();
            }
            sb.AppendFormat("var {0} = function({1}) {{", methodName, string.Join(", ", pNames));
            // Variable declarations
            var declVars = v.vars
                .Select(x => new { name = resolver.LocalVarNames[x], type = x.Type })
                .Where(x => !pNames.Contains(x.name))
                .Select(x => x.name + " = " + DefaultValuer.Get(x.type))
                .Distinct() // Bit of a hack, but works for now
                .ToArray();
            if (declVars.Any()) {
                sb.AppendLine();
                sb.Append(' ', tabSize);
                sb.AppendFormat("var {0};", string.Join(", ", declVars));
            }
            // Method body
            sb.AppendLine(js);
            // Method ending
            sb.AppendLine("}");

            var sbStr = sb.ToString();
            return sbStr;
        }

        private JsMethod(MethodDefinition method, Resolver resolver)
            : base(true) {
            this.method = method;
            this.resolver = resolver;
        }

        private MethodDefinition method;
        private Resolver resolver;

        private Dictionary<ParameterDefinition, ExprVar> parameters = new Dictionary<ParameterDefinition, ExprVar>();
        private StringBuilder js = new StringBuilder();
        private List<ExprVar> vars = new List<ExprVar>();

        private int indent = 1;

        private void NewLine() {
            this.js.Append(Environment.NewLine);
            this.js.Append(' ', this.indent * tabSize);
        }

        protected override ICode VisitBlock(StmtBlock s) {
            foreach (var stmt in s.Statements) {
                this.Visit(stmt);
            }
            return s;
        }

        protected override ICode VisitAssignment(StmtAssignment s) {
            this.NewLine();
            this.Visit(s.Target);
            this.js.Append(" = ");
            this.Visit(s.Expr);
            this.js.Append(";");
            return s;
        }

        protected override ICode VisitAssignment(ExprAssignment e) {
            this.js.Append("(");
            this.Visit(e.Target);
            this.js.Append(" = ");
            this.Visit(e.Expr);
            this.js.Append(")");
            return e;
        }

        protected override ICode VisitVar(ExprVar e) {
            this.js.Append(this.resolver.LocalVarNames[e]);
            // Extra var processing
            this.vars.Add(e);
            if (e.ExprType == Expr.NodeType.VarParameter) {
                if (!this.parameters.ContainsKey(((ExprVarParameter)e).Parameter)) {
                    this.parameters.Add(((ExprVarParameter)e).Parameter, e);
                }
            }
            // Need to visit into phi variables to check for parameters
            if (e.ExprType == Expr.NodeType.VarPhi) {
                var args = ((ExprVarPhi)e).Exprs.Where(x => x.ExprType == Expr.NodeType.VarParameter).Cast<ExprVarParameter>();
                foreach (var arg in args) {
                    if (!this.parameters.ContainsKey(arg.Parameter)) {
                        this.parameters.Add(arg.Parameter, arg);
                    }
                }
            }
            return e;
        }

        private static Dictionary<UnaryOp, string> unaryOps = new Dictionary<UnaryOp, string> {
            { UnaryOp.Not, "!" },
            { UnaryOp.Negate, "-" },
        };
        protected override ICode VisitUnary(ExprUnary e) {
            this.js.Append("(");
            this.js.Append(unaryOps[e.Op]);
            this.Visit(e.Expr);
            this.js.Append(")");
            return e;
        }

        private static Dictionary<BinaryOp, string> binaryOps = new Dictionary<BinaryOp, string> {
            { BinaryOp.Add, "+" },
            { BinaryOp.Sub, "-" },
            { BinaryOp.Mul, "*" },
            { BinaryOp.Div, "/" },
            { BinaryOp.BitwiseAnd, "&" },
            { BinaryOp.BitwiseOr, "|" },
            { BinaryOp.BitwiseXor, "^" },
            { BinaryOp.And, "&&" },
            { BinaryOp.Equal, "===" },
            { BinaryOp.NotEqual, "!==" },
            { BinaryOp.LessThan, "<" },
            { BinaryOp.LessThanOrEqual, "<=" },
            { BinaryOp.GreaterThan, ">" },
            { BinaryOp.GreaterThanOrEqual, ">=" },
            { BinaryOp.Or, "||" },
        };
        protected override ICode VisitBinary(ExprBinary e) {
            var forceInt = e.Op == BinaryOp.Div && e.Type.IsInteger();
            if (forceInt) {
                this.js.Append("(~~");
            }
            this.js.Append("(");
            this.Visit(e.Left);
            this.js.AppendFormat(" {0} ", binaryOps[e.Op]);
            this.Visit(e.Right);
            this.js.Append(")");
            if (forceInt) {
                this.js.Append(")");
            }
            return e;
        }

        protected override ICode VisitTernary(ExprTernary e) {
            this.js.Append("(");
            this.Visit(e.Condition);
            this.js.Append(" ? ");
            this.Visit(e.IfTrue);
            this.js.Append(" : ");
            this.Visit(e.IfFalse);
            this.js.Append(")");
            return e;
        }

        protected override ICode VisitIf(StmtIf s) {
            this.NewLine();
            this.js.Append("if (");
            this.Visit(s.Condition);
            this.js.Append(") {");
            this.indent++;
            this.Visit(s.Then);
            this.indent--;
            if (s.Else != null) {
                this.NewLine();
                this.js.Append("} else {");
                this.indent++;
                this.Visit(s.Else);
                this.indent--;
            }
            this.NewLine();
            this.js.Append("}");
            return s;
        }

        protected override ICode VisitDoLoop(StmtDoLoop s) {
            this.NewLine();
            this.js.Append("do {");
            this.indent++;
            this.Visit(s.Body);
            this.indent--;
            this.NewLine();
            this.js.Append("} while (");
            this.Visit(s.While);
            this.js.Append(");");
            return s;
        }

        protected override ICode VisitSwitch(StmtSwitch s) {
            this.NewLine();
            this.js.Append("switch (");
            this.Visit(s.Expr);
            this.js.Append(") {");
            foreach (var @case in s.Cases) {
                this.NewLine();
                this.js.AppendFormat("case {0}:", @case.Value);
                this.indent++;
                this.Visit(@case.Stmt);
                this.indent--;
            }
            if (s.Default != null) {
                this.NewLine();
                this.js.Append("default:");
                this.indent++;
                this.Visit(s.Default);
                this.indent--;
            }
            this.NewLine();
            this.js.Append("}");
            return s;
        }

        protected override ICode VisitBreak(StmtBreak s) {
            this.NewLine();
            this.js.Append("break;");
            return s;
        }

        protected override ICode VisitLiteral(ExprLiteral e) {
            if (e.Value == null) {
                this.js.Append("null");
            } else {
                object value;
                var mdt = e.Type.MetadataType;
                switch (mdt) {
                case MetadataType.String:
                    value = "\"" + e.Value + "\"";
                    break;
                case MetadataType.Char:
                    value = "\'" + e.Value + "\'";
                    break;
                case MetadataType.Boolean:
                    value = (bool)e.Value ? "true" : "false";
                    break;
                default:
                    value = e.Value;
                    break;
                }
                this.js.Append(value);
            }
            return e;
        }

        protected override ICode VisitDefaultValue(ExprDefaultValue e) {
            var value = DefaultValuer.Get(e.Type);
            this.js.Append(value);
            return e;
        }

        protected override ICode VisitReturn(StmtReturn s) {
            this.NewLine();
            this.js.Append("return");
            if (s.Expr != null) {
                this.js.Append(" ");
                this.Visit(s.Expr);
            }
            this.js.Append(";");
            return s;
        }

        private void CallAppendArgs(ICall e) {
            var mDef = e.CallMethod.Resolve();
            var args = e.Args.ToArray();
            if (!mDef.IsStatic) {
                args = args.Prepend(e.Obj).ToArray();
            }
            this.js.Append("(");
            for (int i = 0; i < args.Length; i++) {
                if (i != 0) {
                    this.js.Append(", ");
                }
                this.Visit(args[i]);
            }
            this.js.Append(")");
        }

        protected override ICode VisitCall(ExprCall e) {
            var mRef = e.CallMethod;
            var mDef = mRef.Resolve();
            if (mDef.DeclaringType.IsInterface) {
                throw new InvalidOperationException("Interface calls should never occur here");
            }
            if (e.IsVirtualCall) {
                throw new InvalidOperationException("Virtual calls should never occur here");
            }
            var name = this.resolver.MethodNames[mRef];
            this.js.Append(name);
            this.CallAppendArgs(e);
            return e;
        }

        protected override ICode VisitJsVirtualCall(ExprJsVirtualCall e) {
            var isIFaceCall = e.CallMethod.DeclaringType.Resolve().IsInterface;
            if (isIFaceCall) {
                var iTableIndex = this.resolver.InterfaceCallIndices[e.CallMethod];
                var iFaceName = this.resolver.InterfaceNames[e.CallMethod.DeclaringType];
                this.Visit(e.ObjInit);
                this.js.AppendFormat("._.{0}[{1}]", iFaceName, iTableIndex);
                this.CallAppendArgs(e);
            } else {
                var mBasemost = e.CallMethod.GetBasemostMethod(null);
                int vTableIndex = this.resolver.VirtualCallIndices[mBasemost];
                this.Visit(e.ObjInit);
                this.js.AppendFormat("._.{0}[{1}]", this.resolver.TypeDataNames[TypeData.VTable], vTableIndex);
                this.CallAppendArgs(e);
            }
            return e;
        }

        protected override ICode VisitFieldAccess(ExprFieldAccess e) {
            if (!e.IsStatic) {
                this.Visit(e.Obj);
                this.js.Append(".");
            }
            this.js.Append(this.resolver.FieldNames[e.Field]);
            return e;
        }

        protected override ICode VisitVarThis(ExprVarThis e) {
            this.js.Append(this.resolver.LocalVarNames[e]);
            return e;
        }

        protected override ICode VisitNewObj(ExprNewObj e) {
            var name = this.resolver.MethodNames[e.CallMethod];
            this.js.Append(name);
            this.js.Append("({_:");
            this.js.Append(this.resolver.TypeNames[e.Type]);
            this.js.Append("}");
            foreach (var arg in e.Args) {
                this.js.Append(", ");
                this.Visit(arg);
            }
            this.js.Append(")");
            return e;
        }

        protected override ICode VisitNewArray(ExprNewArray e) {
            this.js.Append("function() { var _ = new Array(");
            this.Visit(e.ExprNumElements);
            this.js.AppendFormat("); _._ = {0}; for (var i = _.length-1; i >= 0; i--) _[i] = {1}; return _; }}()",
                this.resolver.TypeNames[e.Type], DefaultValuer.Get(e.ElementType));
            return e;
        }

        protected override ICode VisitArrayLength(ExprArrayLength e) {
            this.Visit(e.Array);
            this.js.Append(".length");
            return e;
        }

        protected override ICode VisitVarArrayAccess(ExprVarArrayAccess e) {
            this.Visit(e.Array);
            this.js.Append("[");
            this.Visit(e.Index);
            this.js.Append("]");
            return e;
        }

        protected override ICode VisitTry(StmtTry s) {
            this.NewLine();
            this.js.Append("try {");
            this.indent++;
            this.Visit(s.Try);
            this.indent--;
            foreach (var @catch in s.Catches.EmptyIfNull()) {
                // TODO: Implement full exception processing (need some runtime type information to be able to do this)
                if (!(@catch.ExceptionVar.Type.IsObject() || @catch.ExceptionVar.Type.IsException())) {
                    throw new NotImplementedException("Cannot yet handle 'catch' of type: " + @catch.ExceptionVar.Type.Name);
                }
                this.NewLine();
                this.js.Append("} catch(");
                this.Visit(@catch.ExceptionVar);
                this.js.Append(") {");
                this.indent++;
                this.Visit(@catch.Stmt);
                this.indent--;
            }
            if (s.Finally != null) {
                this.NewLine();
                this.js.Append("} finally {");
                this.indent++;
                this.Visit(s.Finally);
                this.indent--;
            }
            this.NewLine();
            this.js.Append("}");
            return s;
        }

        protected override ICode VisitThrow(StmtThrow s) {
            this.NewLine();
            this.js.Append("throw");
            if (s.Expr != null) {
                this.js.Append(" ");
                this.Visit(s.Expr);
            }
            this.js.Append(";");
            return s;
        }

        protected override ICode VisitWrapExpr(StmtWrapExpr s) {
            this.NewLine();
            this.Visit(s.Expr);
            this.js.Append(";");
            return s;
        }

        protected override ICode VisitCast(ExprCast e) {
            throw new InvalidOperationException("This should never occur");
        }

        protected override ICode VisitIsInst(ExprIsInst e) {
            // TODO: All of it
            throw new Exception();
        }

        protected override ICode VisitEmpty(StmtEmpty s) {
            return s;
        }

        protected override ICode VisitBox(ExprBox e) {
            this.js.Append("{v:");
            this.Visit(e.Expr);
            this.js.Append(",_:");
            this.js.Append(this.resolver.TypeNames[e.Type]);
            this.js.Append("}");
            return e;
        }

        protected override ICode VisitUnbox(ExprUnboxAny e) {
            this.Visit(e.Expr);
            this.js.Append(".v");
            return e;
        }

        protected override ICode VisitRuntimeHandle(ExprRuntimeHandle e) {
            var tokenType = e.Member.MetadataToken.TokenType;
            switch (tokenType) {
            case TokenType.TypeRef:
            case TokenType.TypeDef:
            case TokenType.TypeSpec:
                var type = (TypeReference)e.Member;
                var typeName = this.resolver.TypeNames[type];
                this.js.Append(typeName);
                break;
            default:
                throw new NotImplementedException("Cannot handle runtime-handle type: " + tokenType);
            }
            return e;
        }

        private void HandleExplicitJs(string js, IEnumerable<Expr> exprs) {
            var es = exprs.ToArray();
            int ofs = 0;
            Func<char> getC = () => ofs < js.Length ? js[ofs++] : '\0';
            for (; ; ) {
                var c = getC();
                switch (c) {
                case '\0':
                    return;
                case '{':
                    c = getC();
                    if (c == '{') {
                        this.js.Append('{');
                    } else {
                        if (!char.IsDigit(c)) {
                            throw new InvalidOperationException("Invalid explicit javascript function");
                        }
                        var numStr = c.ToString();
                        for (; ; ) {
                            c = getC();
                            if (c == '}') {
                                break;
                            }
                            if (!char.IsDigit(c)) {
                                throw new InvalidOperationException("Invalid explicit javascript function");
                            }
                            numStr += c;
                        }
                        var num = int.Parse(numStr);
                        var expr = es[num];
                        this.Visit(expr);
                    }
                    break;
                case '}':
                    c = getC();
                    if (c != '}') {
                        throw new InvalidOperationException("Invalid explicit javascript function");
                    }
                    this.js.Append('}');
                    break;
                default:
                    this.js.Append(c);
                    break;
                }
            }
        }

        protected override ICode VisitJsExplicitFunction(StmtJsExplicitFunction s) {
            this.NewLine();
            this.HandleExplicitJs(s.JavaScript, s.Exprs);
            return s;
        }

        protected override ICode VisitJsFunction(ExprJsFunction e) {
            this.js.Append("(function(");
            bool needComma = false;
            foreach (var arg in e.Args) {
                if (needComma) {
                    this.js.Append(", ");
                } else {
                    needComma = true;
                }
                this.Visit(arg);
            }
            this.js.Append(") {");
            this.indent++;
            this.Visit(e.Body);
            this.indent--;
            this.NewLine();
            this.js.Append("})");
            return e;
        }

        protected override ICode VisitJsInvoke(ExprJsInvoke e) {
            this.Visit(e.MethodToInvoke);
            this.js.Append("(");
            bool needComma = false;
            foreach (var arg in e.Args) {
                if (needComma) {
                    this.js.Append(", ");
                } else {
                    needComma = true;
                }
                this.Visit(arg);
            }
            this.js.Append(")");
            return e;
        }

        protected override ICode VisitJsEmptyFunction(ExprJsEmptyFunction e) {
            this.js.Append("function() { }");
            return e;
        }

        protected override ICode VisitJsVarMethodReference(ExprJsVarMethodReference e) {
            this.js.Append(this.resolver.MethodNames[e.MRef]);
            return e;
        }

        protected override ICode VisitJsResolvedProperty(ExprJsResolvedProperty e) {
            if (e.Obj != null) {
                this.Visit(e.Obj);
                this.js.Append(".");
            }
            this.js.Append(e.PropertyName);
            return e;
        }

        protected override ICode VisitJsResolvedMethod(ExprJsResolvedMethod e) {
            if (e.Obj != null) {
                this.Visit(e.Obj);
                this.js.Append(".");
            }
            this.js.Append(e.MethodName);
            this.js.Append("(");
            foreach (var arg in e.Args) {
                this.Visit(arg);
                this.js.Append(", ");
            }
            this.js.Length -= 2;
            this.js.Append(")");
            return e;
        }

        protected override ICode VisitJsArrayLiteral(ExprJsArrayLiteral e) {
            this.js.Append("[");
            foreach (var element in e.Elements) {
                this.Visit(element);
                this.js.Append(", ");
            }
            this.js.Length -= 2;
            this.js.Append("]");
            return e;
        }

        protected override ICode VisitJsTypeData(ExprJsTypeData e) {
            var name = this.resolver.TypeDataNames[e.TypeData];
            this.js.Append(name);
            return e;
        }

        protected override ICode VisitJsTypeVarName(ExprJsTypeVarName e) {
            var name = this.resolver.TypeNames[e.TypeRef];
            this.js.Append(name);
            return e;
        }

        protected override ICode VisitJsExplicit(ExprJsExplicit e) {
            this.HandleExplicitJs(e.JavaScript, e.Exprs);
            return e;
        }

    }
}
