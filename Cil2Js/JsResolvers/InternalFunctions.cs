using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {
    class InternalFunctions {

        // TODO: This way of creating/initialising arrays is not great.
        // A new method will be created for each type of array
        // Although the other option requires getting default value at runtime, also not great
        [Js(typeof(CreateArrayImpl))]
        public static T[] CreateArray<T>(int size) {
            throw new Exception();
        }

        class CreateArrayImpl : IJsImpl {
            public Stmt GetImpl(Ctx ctx) {
                var count = ctx.MethodParameter(0);
                var elType = ((GenericInstanceMethod)ctx.MRef).GenericArguments[0];
                var arrayType = elType.MakeArray();
                var elTypeExpr = new ExprJsTypeVarName(ctx, arrayType);
                var defaultValue = new ExprDefaultValue(ctx, elType);
                var a = new ExprVarLocal(ctx, arrayType);
                var i = new ExprVarLocal(ctx, ctx.Int32);
                var js = "{3}=new Array({0}); {3}._={1}; for({4}={0}-1;{4}>=0;{4}--) {3}[{4}]={2}; return {3};";
                var stmt = new StmtJsExplicit(ctx, js, count, elTypeExpr, defaultValue, a, i);
                return stmt;
            }
        }

        [Js(typeof(CanAssignToImpl))]
        private static bool CanAssignTo(object o, Type toType) {
            throw new Exception();
        }

        class CanAssignToImpl : IJsImpl {
            public Stmt GetImpl(Ctx ctx) {
                var obj = ctx.MethodParameter(0);
                var toType = ctx.MethodParameter(1);
                var canCastTo = new ExprVarLocal(ctx, ctx.Type.MakeArray());
                var mGetType = ctx.Module.Import(typeof(object).GetMethod("GetType"));
                var getTypeCall = new ExprCall(ctx, mGetType, obj);
                var assignableTo = new ExprJsTypeData(ctx, TypeData.AssignableTo);
                var i = new ExprVarLocal(ctx, ctx.Int32);
                var t = new ExprVarLocal(ctx, ctx.Type);
                var js = "if (!{0}) return true; {6}={3}; if ({6}==={1}) return true; {2}={6}.{4}; for ({5}={2}.length-1;{5}>=0;{5}--) if ({2}[{5}]==={1}) return true; return false;";
                var stmt = new StmtJsExplicit(ctx, js, obj, toType, canCastTo, getTypeCall, assignableTo, i, t);
                return stmt;
            }
        }


        [Js(typeof(IsInstImpl))]
        public static object IsInst(object o, Type toType) {
            throw new Exception();
        }

        class IsInstImpl : IJsImpl {
            public Stmt GetImpl(Ctx ctx) {
                var obj = ctx.MethodParameter(0);
                var toType = ctx.MethodParameter(1);
                var callCanAssignTo = new ExprCall(ctx, (Func<object, Type, bool>)CanAssignTo, null, obj, toType);
                var js = "return {0}?{1}:null;";
                var stmt = new StmtJsExplicit(ctx, js, callCanAssignTo, obj);
                return stmt;
            }
        }


        [Js(typeof(CastImpl))]
        public static object Cast(object o, Type toType) {
            throw new Exception();
        }

        class CastImpl : IJsImpl {
            public Stmt GetImpl(Ctx ctx) {
                var obj = ctx.MethodParameter(0);
                var toType = ctx.MethodParameter(1);
                var callCanAssignTo = new ExprCall(ctx, (Func<object, Type, bool>)CanAssignTo, null, obj, toType);
                var mCtorEx = ctx.Module.Import(typeof(InvalidCastException).GetConstructor(new[] { typeof(string) }));
                var mGetType = ctx.Module.Import(typeof(object).GetMethod("GetType"));
                var getTypeCall = new ExprCall(ctx, mGetType, obj);
                var msgParts = new Expr[] {
                    new ExprLiteral(ctx, "Unable to cast object of type '", ctx.String),
                    new ExprCall(ctx, typeof(Type).GetProperty("FullName").GetMethod, getTypeCall),
                    new ExprLiteral(ctx, "' to type '", ctx.String),
                    new ExprCall(ctx, typeof(Type).GetProperty("FullName").GetMethod, toType),
                    new ExprLiteral(ctx, "'.", ctx.String)
                };
                var msg = msgParts.Aggregate((a, b) => new ExprBinary(ctx, BinaryOp.Add, ctx.String, a, b));
                var ctorEx = new ExprNewObj(ctx, mCtorEx, msg);
                var js = "if ({0}) return {1}; throw {2}";
                var stmt = new StmtJsExplicit(ctx, js, callCanAssignTo, obj, ctorEx);
                return stmt;
            }
        }

        [Js(typeof(DeepCopyValueTypeImpl))]
        public static T DeepCopyValueType<T>(T o) {
            throw new Exception();
        }

        class DeepCopyValueTypeImpl : IJsImpl {
            public Stmt GetImpl(Ctx ctx) {
                // Recursively deep-copy a value-type
                var type = ((GenericInstanceMethod)ctx.MRef).GenericArguments[0];
                var fields = type.EnumResolvedFields().Where(x => !x.Resolve().IsStatic).ToArray();
                var o = ctx.MethodParameter(0);
                var eTypeName = new ExprJsTypeVarName(ctx, type);
                var ofsFieldName = 1;
                var ofsFieldValue = ofsFieldName + fields.Length;
                var copy = string.Join(",", fields.Select((x, i) => "{" + (i + ofsFieldName) + "}:{" + (i + ofsFieldValue) + "}"));
                var fieldValues = fields.Select(x => {
                    var fieldAccess = new ExprJsExplicit(ctx, "{0}.{1}", x.FieldType, o, new ExprJsFieldVarName(ctx, x));
                    var dc = ValueTypeDeepCopyIfRequired(x.FieldType, () => fieldAccess);
                    return dc ?? fieldAccess;
                }).ToArray();
                var fieldNames = fields.Select(x => new ExprJsFieldVarName(ctx, x)).ToArray();
                var js = "return {{" + copy + "}}";
                return new StmtJsExplicit(ctx, js, new Expr[] { eTypeName }.Concat(fieldNames).Concat(fieldValues));
            }
        }

        public static Expr ValueTypeDeepCopyIfRequired(TypeReference type, Func<Expr> fnExpr) {
            // If a value-type requires a deep-copy then return an expression that is the deep-copy.
            // Otherwise return null
            if (type.IsValueType && !type.IsPrimitive) {
                var expr = fnExpr();
                var ctx = expr.Ctx;
                var dcGenDef = ((Func<object, object>)DeepCopyValueType).Method.GetGenericMethodDefinition();
                var dc = ctx.Module.Import(dcGenDef);
                var dcGen = dc.MakeGeneric(type);
                var dcCall = new ExprCall(ctx, dcGen, null, expr);
                return dcCall;
            } else {
                return null;
            }
        }

        [Js(typeof(UnboxAnyNullableImpl))]
        public static T? UnboxAnyNullable<T>(object obj) where T : struct {
            throw new Exception();
        }

        class UnboxAnyNullableImpl : IJsImpl {
            public Stmt GetImpl(Ctx ctx) {
                // If obj==null create Nullable with hasValue=false
                // If obj.Type not assignable to e.InnerType throw InvalidCastEx
                var obj = ctx.MethodParameter(0);
                var type = ((GenericInstanceMethod)ctx.MRef).GenericArguments[0];
                var nType = type.MakeNullable();
                var nameHasValue = new ExprJsFieldVarName(ctx, nType.GetField("hasValue"));
                var nameValue = new ExprJsFieldVarName(ctx, nType.GetField("value"));
                var defaultValue = new ExprDefaultValue(ctx, type);
                var canAssign = new ExprCall(ctx, ((Func<object, Type, bool>)CanAssignTo).Method, null, obj, new ExprJsTypeVarName(ctx, type));
                var invCastExCtor = ctx.Module.Import(typeof(InvalidCastException).GetConstructor(new[] { typeof(string) }));
                var invCastEx = new ExprNewObj(ctx, invCastExCtor, new ExprLiteral(ctx, "Specified cast is not valid.", ctx.String));
                var js = "if (!{0}) return {{{1}:false,{2}:{3}}}; if (!{4}) throw {5}; return {{{1}:true,{2}:{0}.v}};";
                var stmt = new StmtJsExplicit(ctx, js, obj, nameHasValue, nameValue, defaultValue, canAssign, invCastEx);
                return stmt;
            }
        }

        [Js(typeof(UnboxAnyNonNullableImpl))]
        public static T UnboxAnyNonNullable<T>(object obj) where T : struct {
            throw new Exception();
        }

        class UnboxAnyNonNullableImpl : IJsImpl {
            public Stmt GetImpl(Ctx ctx) {
                // If obj==null throw NullRefEx
                // If obj.Type not assignable to e.Type throw InvalidCastEx
                // otherwise unbox
                var obj = ctx.MethodParameter(0);
                var type = ((GenericInstanceMethod)ctx.MRef).GenericArguments[0];
                var nullRefExCtor = ctx.Module.Import(typeof(NullReferenceException).GetConstructor(new[] { typeof(string) }));
                var nullRefEx = new ExprNewObj(ctx, nullRefExCtor, new ExprLiteral(ctx, "Object reference not set to an instance of an object.", ctx.String));
                var canAssign = new ExprCall(ctx, ((Func<object, Type, bool>)CanAssignTo).Method, null, obj, new ExprJsTypeVarName(ctx, type));
                var invCastExCtor = ctx.Module.Import(typeof(InvalidCastException).GetConstructor(new[] { typeof(string) }));
                var invCastEx = new ExprNewObj(ctx, invCastExCtor, new ExprLiteral(ctx, "Specified cast is not valid.", ctx.String));
                var js = "if (!{0}) throw {1}; if (!{2}) throw {3}; return {0}.v;";
                var stmt = new StmtJsExplicit(ctx, js, obj, nullRefEx, canAssign, invCastEx);
                return stmt;
            }
        }

        [Js(typeof(ConvImpl))]
        public static TOut Conv<TIn, TOut>(TIn v) {
            throw new Exception();
        }

        class ConvImpl : IJsImpl {
            public Stmt GetImpl(Ctx ctx) {
                // Generic parameters can only be:
                // TIn = Int32: TOut = UInt64
                // TIn = Int64: TOut = UInt64
                var mGenInst = (GenericInstanceMethod)ctx.MRef;
                var tFrom = mGenInst.GenericArguments[0];
                var tTo = mGenInst.GenericArguments[1];
                var v = ctx.MethodParameter(0);
                string js;
                bool useTemp = false;
                switch (tFrom.MetadataType) {
                case MetadataType.SByte:
                    switch (tTo.MetadataType) {
                    case MetadataType.Byte:
                        js = "return {0}>=0?{0}:" + (Byte.MaxValue + 1) + "+{0};";
                        break;
                    case MetadataType.UInt16:
                        js = "return {0}>=0?{0}:" + (UInt16.MaxValue + 1) + "+{0};";
                        break;
                    case MetadataType.UInt32:
                        js = "return {0}>=0?{0}:" + (UInt32.MaxValue + 1L) + "+{0};";
                        break;
                    case MetadataType.UInt64:
                        js = "return {0}>=0?{0}:" + UInt64.MaxValue + "+{0}+1;";
                        break;
                    default:
                        throw new NotImplementedException("Cannot handle TOut: " + tTo.MetadataType);
                    }
                    break;
                case MetadataType.Byte:
                    switch (tTo.MetadataType) {
                    case MetadataType.SByte:
                        js = "return {0}<=" + SByte.MaxValue + "?{0}:{0}-" + (Byte.MaxValue + 1) + ";";
                        break;
                    default:
                        throw new NotImplementedException("Cannot handle TOut: " + tTo.MetadataType);
                    }
                    break;
                case MetadataType.Int16:
                    switch (tTo.MetadataType) {
                    case MetadataType.UInt32:
                        js = "return {0}>=0?{0}:" + (UInt32.MaxValue + 1L) + "+{0};";
                        break;
                    case MetadataType.UInt64:
                        js = "return {0}>=0?{0}:" + UInt64.MaxValue + "+{0}+1;";
                        break;
                    default:
                        throw new NotImplementedException("Cannot handle TOut: " + tTo.MetadataType);
                    }
                    break;
                case MetadataType.Int32:
                    switch (tTo.MetadataType) {
                    case MetadataType.UInt32:
                    case MetadataType.UInt64:
                        js = "return {0}>=0?{0}:" + (UInt32.MaxValue + 1L) + "+{0};";
                        break;
                    default:
                        throw new NotImplementedException("Cannot handle TOut: " + tTo.MetadataType);
                    }
                    break;
                case MetadataType.Int64:
                    switch (tTo.MetadataType) {
                    case MetadataType.UInt64:
                        js = "return {0}>=0?{0}:" + UInt64.MaxValue + "+{0}+1;";
                        break;
                    default:
                        throw new NotImplementedException("Cannot handle TOut: " + tTo.MetadataType);
                    }
                    break;
                case MetadataType.UInt64:
                    switch (tTo.MetadataType) {
                    case MetadataType.Int64:
                        js = "return {0}<=" + Int64.MaxValue + "?{0}:{0}-" + ((UInt64)Int64.MaxValue + 1L) + ";";
                        break;
                    case MetadataType.UInt32:
                        useTemp = true;
                        js = "{1}={0}&-1;return {1}>=0?{1}:" + (UInt32.MaxValue + 1L) + "+{1};";
                        break;
                    default:
                        throw new NotImplementedException("Cannot handle TOut: " + tTo.MetadataType);
                    }
                    break;
                default:
                    throw new NotImplementedException("Cannot handle TIn: " + tFrom.MetadataType);
                }
                var stmt = new StmtJsExplicit(ctx, js, v, useTemp ? new ExprVarLocal(ctx, ctx.Object) : null);
                return stmt;
            }
        }


    }
}
