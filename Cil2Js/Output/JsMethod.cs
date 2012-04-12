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

        public const int tabSize = 4;

        public static string Create(MethodReference mRef, Resolver resolver, ICode ast) {
            if (mRef.ContainsGenericParameters()) {
                throw new ArgumentException("Cannot create JS for method with open generic parameters");
            }
            var mDef = mRef.Resolve();
            if (mDef.IsAbstract) {
                throw new ArgumentException("Should never need to transcode an abstract method");
            }
            var tRef = mRef.DeclaringType;
            var tDef = tRef.Resolve();

            var v = new JsMethod(resolver);
            v.Visit(ast);
            var js = v.js.ToString();

            var sb = new StringBuilder();
            // Method declaration
            var methodName = resolver.MethodNames[mRef];
            //var parameterNames = mRef.Parameters.Select(x => v.parameters.ValueOrDefault(x).NullThru(y => resolver.LocalVarNames[y])).ToArray();
            // Match parameters, but have to do by position, as method built may be a custom method replacing a BCL method,
            // so parameters are not the same.
            var parameterNames = mRef.Parameters.Select(x => v.parameters.FirstOrDefault(y => y.Key.Sequence == x.Sequence).Value.NullThru(y => resolver.LocalVarNames[y])).ToArray();
            if (!mDef.IsStatic) {
                var thisName = v.vars.FirstOrDefault(x => x.ExprType == Expr.NodeType.VarThis).NullThru(x => resolver.LocalVarNames[x]);
                parameterNames = parameterNames.Prepend(thisName).ToArray();
            }
            var unusedParameterNameGen = new NameGenerator();
            parameterNames = parameterNames.Select(x => x ?? ("_" + unusedParameterNameGen.GetNewName())).ToArray();
            sb.AppendFormat("// {0}", mRef.FullName);
            sb.AppendLine();
            sb.AppendFormat("var {0} = function({1}) {{", methodName, string.Join(", ", parameterNames));
            // Variable declarations
            var declVars = v.vars
                .Select(x => new { name = resolver.LocalVarNames[x], type = x.Type })
                .Where(x => !parameterNames.Contains(x.name))
                .Select(x => {
                    var name = x.name;
                    if (x.type.IsValueType) {
                        name += " = " + DefaultValuer.Get(x.type, resolver.FieldNames);
                    }
                    return name;
                })
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

        private JsMethod(Resolver resolver)
            : base(true) {
            this.resolver = resolver;
        }

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
            // TODO: The special processing for ByRef vars shouldn't be needed - it should be handled elsewhere
            this.NewLine();
            this.Visit(s.Target);
            if (s.Target.Type.IsByReference && !s.Expr.Type.IsByReference) {
                this.js.Append("[0]");
            }
            this.js.Append(" = ");
            this.Visit(s.Expr);
            if (!s.Target.Type.IsByReference && s.Expr.Type.IsByReference) {
                this.js.Append("[0]");
            }
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
            { UnaryOp.BitwiseNot, "~" },
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
            { BinaryOp.Div_Un, "/" },
            { BinaryOp.Rem, "%" },
            { BinaryOp.Rem_Un, "%" },
            { BinaryOp.Shl, "<<" },
            { BinaryOp.BitwiseAnd, "&" },
            { BinaryOp.BitwiseOr, "|" },
            { BinaryOp.BitwiseXor, "^" },
            { BinaryOp.And, "&&" },
            { BinaryOp.Equal, "===" },
            { BinaryOp.NotEqual, "!==" },
            { BinaryOp.LessThan, "<" },
            { BinaryOp.LessThan_Un, "<" },
            { BinaryOp.LessThanOrEqual, "<=" },
            { BinaryOp.LessThanOrEqual_Un, "<=" },
            { BinaryOp.GreaterThan, ">" },
            { BinaryOp.GreaterThan_Un, ">" },
            { BinaryOp.GreaterThanOrEqual, ">=" },
            { BinaryOp.GreaterThanOrEqual_Un, ">=" },
            { BinaryOp.Or, "||" },
        };
        protected override ICode VisitBinary(ExprBinary e) {
            var forceInt = e.Op == BinaryOp.Div && e.Type.IsInteger();
            if (forceInt) {
                if (e.Type.IsUInt32()) {
                    this.js.Append("(");
                } else {
                    this.js.Append("(~~");
                }
            }
            this.js.Append("(");
            this.Visit(e.Left);
            this.js.AppendFormat(" {0} ", binaryOps[e.Op]);
            this.Visit(e.Right);
            this.js.Append(")");
            if (forceInt) {
                if (e.Type.IsUInt32()) {
                    this.js.Append(">>>0");
                } else {
                    this.js.Append(")");
                }
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

        protected override ICode VisitStoreObj(StmtStoreObj s) {
            this.NewLine();
            switch (s.Destination.ExprType) {
            case Expr.NodeType.ElementAddress:
                this.Visit(s.Destination);
                break;
            case Expr.NodeType.VarLocal:
            case Expr.NodeType.VarParameter:
                this.Visit(s.Destination);
                this.js.Append("[0]");
                break;
            default:
                throw new NotImplementedException("Cannot handle expr-type: " + s.Destination.ExprType);
            }
            this.js.Append(" = ");
            this.Visit(s.Source);
            this.js.Append(";");
            return s;
        }

        protected override ICode VisitInitObj(StmtInitObj s) {
            this.NewLine();
            this.Visit(s.Destination);
            this.js.Append(" = ");
            var defaultValue = DefaultValuer.Get(s.Type, this.resolver.FieldNames);
            this.js.Append(defaultValue);
            this.js.Append(";");
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
                    var s = (string)e.Value;
                    var sb = new StringBuilder(s.Length + 2);
                    sb.Append("\"");
                    for (int i = 0; i < s.Length; i++) {
                        var c = s[i];
                        if ((int)c >= 32 && c <= 126 && c != '\\' && c != '\'' && c != '"') {
                            sb.Append(c);
                        } else {
                            if ((int)c <= 255) {
                                sb.AppendFormat("\\x{0:x2}", (int)c);
                            } else {
                                sb.AppendFormat("\\u{0:x4}", (int)c);
                            }
                        }
                    }
                    sb.Append("\"");
                    value = sb.ToString();
                    //s = s.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\"", "\\\"");
                    //value = "\"" + s + "\"";
                    break;
                case MetadataType.Char:
                    value = (int)(char)e.Value;
                    break;
                case MetadataType.Boolean:
                    value = (bool)e.Value ? "true" : "false";
                    break;
                case MetadataType.Int64:
                    var i64 = (UInt64)(Int64)e.Value;
                    value = string.Format("[{0}, {1}]", (i64 >> 32) & 0xffffffffUL, i64 & 0xffffffffUL);
                    break;
                case MetadataType.UInt64:
                    var u64 = (UInt64)e.Value;
                    value = string.Format("[{0}, {1}]", (u64 >> 32) & 0xffffffffUL, u64 & 0xffffffffUL);
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
            var value = DefaultValuer.Get(e.Type, this.resolver.FieldNames);
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
                this.Visit(e.RuntimeType);
                this.js.AppendFormat(".{0}[{1}]", iFaceName, iTableIndex);
                this.CallAppendArgs(e);
            } else {
                var mBasemost = e.CallMethod.GetBasemostMethod(null);
                int vTableIndex = this.resolver.VirtualCallIndices[mBasemost];
                this.Visit(e.RuntimeType);
                this.js.AppendFormat(".{0}[{1}]", this.resolver.TypeDataNames[TypeData.VTable], vTableIndex);
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

        protected override ICode VisitFieldAddress(ExprFieldAddress e) {
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
            this.js.Append("({");
            if (!e.Type.IsValueType) {
                this.js.Append("_:");
                this.js.Append(this.resolver.TypeNames[e.Type]);
            }
            this.js.Append("}");
            foreach (var arg in e.Args) {
                this.js.Append(", ");
                this.Visit(arg);
            }
            this.js.Append(")");
            return e;
        }

        protected override ICode VisitNewArray(ExprNewArray e) {
            throw new InvalidOperationException("Should never get here");
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

        protected override ICode VisitElementAddress(ExprElementAddress e) {
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

        protected override ICode VisitConv(ExprConv e) {
            throw new InvalidOperationException("This should never occur");
        }

        protected override ICode VisitCast(ExprCast e) {
            throw new InvalidOperationException("This should never occur");
        }

        protected override ICode VisitIsInst(ExprIsInst e) {
            throw new InvalidOperationException("This should never occur");
        }

        protected override ICode VisitEmpty(StmtEmpty s) {
            return s;
        }

        protected override ICode VisitBox(ExprBox e) {
            throw new InvalidOperationException("This should never occur");
        }

        protected override ICode VisitUnboxAny(ExprUnboxAny e) {
            throw new InvalidOperationException("This should never occur");
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

        protected override ICode VisitVariableAddress(ExprVariableAddress e) {
            this.Visit(e.Variable);
            return e;
            //throw new InvalidOperationException("Should never get here");
        }

        protected override ICode VisitArgAddress(ExprArgAddress e) {
            this.Visit(e.Arg);
            return e;
        }

        protected override ICode VisitLoadIndirect(ExprLoadIndirect e) {
            this.Visit(e.Expr);
            return e;
        }

        private void HandleExplicitJs(string js, IEnumerable<NamedExpr> exprs) {
            js = js.Trim();
            var es = exprs.Where(x => x != null).ToDictionary(x => x.Name, x => x.Expr);
            int ofs = 0;
            Func<char> getC = () => ofs < js.Length ? js[ofs++] : '\0';
            var cur = new StringBuilder();
            for (; ; ) {
                var c = getC();
                var isName = char.IsLetter(c) || (char.IsDigit(c) && cur.Length > 0);
                if (isName) {
                    cur.Append(c);
                } else {
                    if (cur.Length > 0) {
                        var curS = cur.ToString();
                        var mapped = es.ValueOrDefault(curS);
                        if (mapped != null) {
                            this.Visit(mapped);
                        } else {
                            this.js.Append(curS);
                        }
                        cur.Length = 0;
                    }
                    if (c != '\0' && c != '\r') {
                        if (c == '\n') {
                            this.NewLine();
                        } else {
                            this.js.Append(c);
                        }
                    }
                }
                if (c == '\0') {
                    return;
                }
            }
        }

        protected override ICode VisitJsExplicit(StmtJsExplicit s) {
            this.NewLine();
            this.HandleExplicitJs(s.JavaScript, s.NamedExprs);
            return s;
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
            if (e.PropertyName == null) {
                // Indexer
                this.Visit(e.Call.Obj);
                this.js.Append("[");
                this.Visit(e.Call.Args.First());
                this.js.Append("]");
            } else {
                // Normal attribute/property
                if (e.Call.Obj != null) {
                    this.Visit(e.Call.Obj);
                    this.js.Append(".");
                }
                this.js.Append(e.PropertyName);
            }
            if (e.Call.CallMethod != null && e.Call.CallMethod.Resolve().IsSetter) {
                this.js.Append(" = ");
                this.Visit(e.Call.Args.Last());
            }
            return e;
        }

        protected override ICode VisitJsResolvedMethod(ExprJsResolvedMethod e) {
            if (e.Obj != null) {
                this.Visit(e.Obj);
                this.js.Append(".");
            }
            this.js.Append(e.MethodName);
            this.js.Append("(");
            if (e.Args.Any()) {
                foreach (var arg in e.Args) {
                    this.Visit(arg);
                    this.js.Append(", ");
                }
                this.js.Length -= 2;
            }
            this.js.Append(")");
            return e;
        }

        protected override ICode VisitJsResolvedCtor(ExprJsResolvedCtor e) {
            this.js.Append("new ");
            this.js.Append(e.TypeName);
            this.js.Append("(");
            if (e.Args.Any()) {
                foreach (var arg in e.Args) {
                    this.Visit(arg);
                    this.js.Append(", ");
                }
                this.js.Length -= 2;
            }
            this.js.Append(")");
            return e;
        }

        protected override ICode VisitJsArrayLiteral(ExprJsArrayLiteral e) {
            this.js.Append("[");
            if (e.Elements.Any()) {
                foreach (var element in e.Elements) {
                    this.Visit(element);
                    this.js.Append(", ");
                }
                this.js.Length -= 2;
            }
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
            this.HandleExplicitJs(e.JavaScript, e.NamedExprs);
            return e;
        }

        protected override ICode VisitJsFieldVarName(ExprJsFieldVarName e) {
            var name = this.resolver.FieldNames[e.FieldRef];
            this.js.Append(name);
            return e;
        }

        protected override ICode VisitJsDelegateCtor(ExprJsDelegateCtor e) {
            var methodName = this.resolver.MethodNames[e.Method];
            if (e.Obj == null) {
                this.js.Append(methodName);
            } else {
                // TODO improve naming - can share names with enclosing function
                var nameGen = new NameGenerator();
                var argList = string.Join(", ", e.Method.Parameters.Select(x => "_" + nameGen.GetNewName()));
                this.js.Append("(function(");
                this.js.Append(argList);
                this.js.Append(") { ");
                if (!e.Type.IsVoid()) {
                    this.js.Append("return ");
                }
                this.js.Append(methodName);
                this.js.Append("(");
                this.Visit(e.Obj);
                if (argList.Any()) {
                    this.js.Append(", ");
                    this.js.Append(argList);
                }
                this.js.Append("); })");
            }
            return e;
        }

        protected override ICode VisitJsDelegateInvoke(ExprJsDelegateInvoke e) {
            this.Visit(e.MethodToInvoke);
            this.js.Append("(");
            if (e.Args.Any()) {
                foreach (var arg in e.Args) {
                    this.Visit(arg);
                    this.js.Append(", ");
                }
                this.js.Length -= 2;
            }
            this.js.Append(")");
            return e;
        }

        protected override ICode VisitJsByRefWrapper(ExprJsByRefWrapper e) {
            this.js.Append("(");
            foreach (var byRef in e.ByRefs) {
                this.Visit(byRef.Item2);
                this.js.Append(" = [");
                this.Visit(byRef.Item1);
                this.js.Append("], ");
            }
            this.Visit(e.ResultTemp);
            this.js.Append(" = ");
            this.Visit(e.Expr);
            foreach (var byRef in e.ByRefs) {
                this.js.Append(", ");
                this.Visit(byRef.Item1);
                this.js.Append(" = ");
                this.Visit(byRef.Item2);
                this.js.Append("[0]");
            }
            this.js.Append(", ");
            this.Visit(e.ResultTemp);
            this.js.Append(")");
            return e;
        }

    }
}
