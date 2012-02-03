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
                var count = ctx.MethodParameter(0, "count");
                var elType = ((GenericInstanceMethod)ctx.MRef).GenericArguments[0];
                var arrayType = elType.MakeArray();
                var elTypeExpr = new ExprJsTypeVarName(ctx, arrayType).Named("type");
                var defaultValue = new ExprDefaultValue(ctx, elType).Named("defaultValue");
                var a = new ExprVarLocal(ctx, arrayType).Named("a");
                var i = new ExprVarLocal(ctx, ctx.Int32).Named("i");
                var js = "a=new Array(count); a._=type; for(i=count-1;i>=0;i--) a[i]=defaultValue; return a;";
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
                var obj = ctx.MethodParameter(0, "obj");
                var toType = ctx.MethodParameter(1, "toType");
                var canCastTo = new ExprVarLocal(ctx, ctx.Type.MakeArray()).Named("canCastTo");
                var mGetType = ctx.Module.Import(typeof(object).GetMethod("GetType"));
                var getTypeCall = new ExprCall(ctx, mGetType, obj.Expr).Named("getTypeCall");
                var assignableTo = new ExprJsTypeData(ctx, TypeData.AssignableTo).Named("assignableTo");
                var i = new ExprVarLocal(ctx, ctx.Int32).Named("i");
                var t = new ExprVarLocal(ctx, ctx.Type).Named("temp");
                var js = "if (!obj) return true; temp=getTypeCall; if (temp===toType) return true; canCastTo=temp.assignableTo; for (i=canCastTo.length-1;i>=0;i--) if (canCastTo[i]===toType) return true; return false;";
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
                var obj = ctx.MethodParameter(0, "obj");
                var toType = ctx.MethodParameter(1);
                var callCanAssignTo = new ExprCall(ctx, (Func<object, Type, bool>)CanAssignTo, null, obj.Expr, toType).Named("callCanAssignTo");
                var js = "return callCanAssignTo?obj:null;";
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
                var obj = ctx.MethodParameter(0, "obj");
                var toType = ctx.MethodParameter(1);
                var callCanAssignTo = new ExprCall(ctx, (Func<object, Type, bool>)CanAssignTo, null, obj.Expr, toType).Named("callCanAssignTo");
                var mCtorEx = ctx.Module.Import(typeof(InvalidCastException).GetConstructor(new[] { typeof(string) }));
                var mGetType = ctx.Module.Import(typeof(object).GetMethod("GetType"));
                var getTypeCall = new ExprCall(ctx, mGetType, obj.Expr);
                var msgParts = new Expr[] {
                    new ExprLiteral(ctx, "Unable to cast object of type '", ctx.String),
                    new ExprCall(ctx, typeof(Type).GetProperty("FullName").GetMethod, getTypeCall),
                    new ExprLiteral(ctx, "' to type '", ctx.String),
                    new ExprCall(ctx, typeof(Type).GetProperty("FullName").GetMethod, toType),
                    new ExprLiteral(ctx, "'.", ctx.String)
                };
                var msg = msgParts.Aggregate((a, b) => new ExprBinary(ctx, BinaryOp.Add, ctx.String, a, b));
                var ctorEx = new ExprNewObj(ctx, mCtorEx, msg).Named("callCtorEx");
                var js = "if (callCanAssignTo) return obj; throw callCtorEx";
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
                var o = ctx.MethodParameter(0, "o");
                var copy = string.Join(",", fields.Select((x, i) => string.Format("f{0}:v{0}", i)));
                var fieldNames = fields.Select((x, i) => new ExprJsFieldVarName(ctx, x).Named("f" + i)).ToArray();
                var fieldValues = fields.Select((x, i) => {
                    var fieldAccess = new ExprJsExplicit(ctx, "o.field", x.FieldType, o, new ExprJsFieldVarName(ctx, x).Named("field"));
                    var dc = ValueTypeDeepCopyIfRequired(x.FieldType, () => fieldAccess);
                    return (dc ?? fieldAccess).Named("v" + i);
                }).ToArray();
                var js = "return {" + copy + "}";
                return new StmtJsExplicit(ctx, js, fieldNames.Concat(fieldValues));
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
                var obj = ctx.MethodParameter(0, "obj");
                var type = ((GenericInstanceMethod)ctx.MRef).GenericArguments[0];
                var nType = type.MakeNullable();
                var nameHasValue = new ExprJsFieldVarName(ctx, nType.GetField("hasValue")).Named("hasValue");
                var nameValue = new ExprJsFieldVarName(ctx, nType.GetField("value")).Named("value");
                var defaultValue = new ExprDefaultValue(ctx, type).Named("defaultValue");
                var canAssign = new ExprCall(ctx, ((Func<object, Type, bool>)CanAssignTo).Method, null, obj.Expr, new ExprJsTypeVarName(ctx, type)).Named("canAssignCall");
                var invCastExCtor = ctx.Module.Import(typeof(InvalidCastException).GetConstructor(new[] { typeof(string) }));
                var invCastEx = new ExprNewObj(ctx, invCastExCtor, new ExprLiteral(ctx, "Specified cast is not valid.", ctx.String)).Named("invCastEx");
                var js = "if (!obj) return {hasValue:false,value:defaultValue}; if (!canAssignCall) throw invCastEx; return {hasValue:true,value:obj.v};";
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
                var obj = ctx.MethodParameter(0, "obj");
                var type = ((GenericInstanceMethod)ctx.MRef).GenericArguments[0];
                var nullRefExCtor = ctx.Module.Import(typeof(NullReferenceException).GetConstructor(new[] { typeof(string) }));
                var nullRefEx = new ExprNewObj(ctx, nullRefExCtor, new ExprLiteral(ctx, "Object reference not set to an instance of an object.", ctx.String)).Named("nullRefEx");
                var canAssign = new ExprCall(ctx, ((Func<object, Type, bool>)CanAssignTo).Method, null, obj.Expr, new ExprJsTypeVarName(ctx, type)).Named("canAssignCall");
                var invCastExCtor = ctx.Module.Import(typeof(InvalidCastException).GetConstructor(new[] { typeof(string) }));
                var invCastEx = new ExprNewObj(ctx, invCastExCtor, new ExprLiteral(ctx, "Specified cast is not valid.", ctx.String)).Named("invCastEx");
                var js = "if (!obj) throw nullRefEx; if (!canAssignCall) throw invCastEx; return obj.v;";
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
                var v = ctx.MethodParameter(0, "v");
                string js;
                bool useTemp = false;
                switch (tFrom.MetadataType) {
                case MetadataType.SByte:
                    switch (tTo.MetadataType) {
                    case MetadataType.Byte:
                        js = "return v>=0?v:" + (Byte.MaxValue + 1) + "+v;";
                        break;
                    case MetadataType.UInt16:
                        js = "return v>=0?v:" + (UInt16.MaxValue + 1) + "+v;";
                        break;
                    case MetadataType.UInt32:
                        js = "return v>=0?v:" + (UInt32.MaxValue + 1L) + "+v;";
                        break;
                    case MetadataType.UInt64:
                        js = "return v>=0?v:" + UInt64.MaxValue + "+v+1;";
                        break;
                    default:
                        throw new NotImplementedException("Cannot handle TOut: " + tTo.MetadataType);
                    }
                    break;
                case MetadataType.Byte:
                    switch (tTo.MetadataType) {
                    case MetadataType.SByte:
                        js = "return v<=" + SByte.MaxValue + "?v:v-" + (Byte.MaxValue + 1) + ";";
                        break;
                    default:
                        throw new NotImplementedException("Cannot handle TOut: " + tTo.MetadataType);
                    }
                    break;
                case MetadataType.Int16:
                    switch (tTo.MetadataType) {
                    case MetadataType.UInt32:
                        js = "return v>=0?v:" + (UInt32.MaxValue + 1L) + "+v;";
                        break;
                    case MetadataType.UInt64:
                        js = "return v>=0?v:" + UInt64.MaxValue + "+v+1;";
                        break;
                    default:
                        throw new NotImplementedException("Cannot handle TOut: " + tTo.MetadataType);
                    }
                    break;
                case MetadataType.Int32:
                    switch (tTo.MetadataType) {
                    case MetadataType.UInt32:
                    case MetadataType.UInt64:
                        js = "return v>=0?v:" + (UInt32.MaxValue + 1L) + "+v;";
                        break;
                    default:
                        throw new NotImplementedException("Cannot handle TOut: " + tTo.MetadataType);
                    }
                    break;
                case MetadataType.Int64:
                    switch (tTo.MetadataType) {
                    case MetadataType.UInt64:
                        js = "return v>=0?v:" + UInt64.MaxValue + "+v+1;";
                        break;
                    default:
                        throw new NotImplementedException("Cannot handle TOut: " + tTo.MetadataType);
                    }
                    break;
                case MetadataType.UInt64:
                    switch (tTo.MetadataType) {
                    case MetadataType.Int64:
                        js = "return v<=" + Int64.MaxValue + "?v:v-" + ((UInt64)Int64.MaxValue + 1L) + ";";
                        break;
                    case MetadataType.UInt32:
                        useTemp = true;
                        js = "temp=v&-1;return temp>=0?temp:" + (UInt32.MaxValue + 1L) + "+temp;";
                        break;
                    default:
                        throw new NotImplementedException("Cannot handle TOut: " + tTo.MetadataType);
                    }
                    break;
                case MetadataType.Single:
                case MetadataType.Double:
                    switch (tTo.MetadataType) {
                    case MetadataType.UInt32:
                        useTemp = true;
                        js = "temp=~~v;return temp>=0?temp:" + (UInt32.MaxValue + 1L) + "+temp;";
                        break;
                    default:
                        throw new NotImplementedException("Cannot handle TOut: " + tTo.MetadataType);
                    }
                    break;
                default:
                    throw new NotImplementedException("Cannot handle TIn: " + tFrom.MetadataType);
                }
                var stmt = new StmtJsExplicit(ctx, js, v, useTemp ? new ExprVarLocal(ctx, ctx.Object).Named("temp") : null);
                return stmt;
            }
        }


    }
}
