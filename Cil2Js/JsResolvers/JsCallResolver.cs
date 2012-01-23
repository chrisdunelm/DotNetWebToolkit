using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Output;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;
using DotNetWebToolkit.Attributes;
using DotNetWebToolkit.Cil2Js.JsResolvers.Methods;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {
    public static partial class JsResolver {

        private static Dictionary<M, Func<ICall, Expr>> callMap = new Dictionary<M, Func<ICall, Expr>>(M.ValueEqComparer) {
            { M.Def(TVoid, "System.Action..ctor", TObject, TIntPtr), ResolverSystem.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`1..ctor", TObject, TIntPtr), ResolverSystem.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`2..ctor", TObject, TIntPtr), ResolverSystem.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`3..ctor", TObject, TIntPtr), ResolverSystem.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`4..ctor", TObject, TIntPtr), ResolverSystem.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`5..ctor", TObject, TIntPtr), ResolverSystem.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`6..ctor", TObject, TIntPtr), ResolverSystem.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`7..ctor", TObject, TIntPtr), ResolverSystem.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`8..ctor", TObject, TIntPtr), ResolverSystem.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action.Invoke"), ResolverSystem.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`1.Invoke", (string)null), ResolverSystem.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`2.Invoke", (string)null, null), ResolverSystem.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`3.Invoke", (string)null, null, null), ResolverSystem.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`4.Invoke", (string)null, null, null, null), ResolverSystem.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`5.Invoke", (string)null, null, null, null, null), ResolverSystem.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`6.Invoke", (string)null, null, null, null, null, null), ResolverSystem.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`7.Invoke", (string)null, null, null, null, null, null, null), ResolverSystem.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`8.Invoke", (string)null, null, null, null, null, null, null, null), ResolverSystem.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Func`1..ctor", TObject, TIntPtr), ResolverSystem.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`2..ctor", TObject, TIntPtr), ResolverSystem.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`3..ctor", TObject, TIntPtr), ResolverSystem.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`4..ctor", TObject, TIntPtr), ResolverSystem.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`5..ctor", TObject, TIntPtr), ResolverSystem.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`6..ctor", TObject, TIntPtr), ResolverSystem.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`7..ctor", TObject, TIntPtr), ResolverSystem.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`8..ctor", TObject, TIntPtr), ResolverSystem.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`9..ctor", TObject, TIntPtr), ResolverSystem.ActionFunc_ctor },
            { M.Def(null, "System.Func`1.Invoke"), ResolverSystem.ActionFunc_Invoke },
            { M.Def(null, "System.Func`2.Invoke", (string)null), ResolverSystem.ActionFunc_Invoke },
            { M.Def(null, "System.Func`3.Invoke", (string)null, null), ResolverSystem.ActionFunc_Invoke },
            { M.Def(null, "System.Func`4.Invoke", (string)null, null, null), ResolverSystem.ActionFunc_Invoke },
            { M.Def(null, "System.Func`5.Invoke", (string)null, null, null, null), ResolverSystem.ActionFunc_Invoke },
            { M.Def(null, "System.Func`6.Invoke", (string)null, null, null, null, null), ResolverSystem.ActionFunc_Invoke },
            { M.Def(null, "System.Func`7.Invoke", (string)null, null, null, null, null, null), ResolverSystem.ActionFunc_Invoke },
            { M.Def(null, "System.Func`8.Invoke", (string)null, null, null, null, null, null, null), ResolverSystem.ActionFunc_Invoke },
            { M.Def(null, "System.Func`9.Invoke", (string)null, null, null, null, null, null, null, null), ResolverSystem.ActionFunc_Invoke },

            { M.Def(TBoolean, "System.String.op_Equality", TString, TString), ResolverString.op_Equality },
            { M.Def(TInt32, "System.String.get_Length"), ResolverString.get_Length },
            { M.Def(TChar, "System.String.get_Chars", TInt32), ResolverString.get_Chars },
            { M.Def(TString, "System.String.Concat", TString, TString), ResolverString.ConcatStrings },
            { M.Def(TString, "System.String.Concat", TString, TString, TString), ResolverString.ConcatStrings },
            { M.Def(TString, "System.String.Concat", TString, TString, TString, TString), ResolverString.ConcatStrings },
            { M.Def(TString, "System.String.Concat", ArrayOf(TString)), ResolverString.ConcatStringsMany },
            { M.Def(TInt32, "System.String.IndexOf", TChar), ResolverString.IndexOfChar },
            { M.Def(TInt32, "System.String.IndexOf", TString), ResolverString.IndexOfString },
            { M.Def(TInt32, "System.String.IndexOf", TChar, TInt32), ResolverString.IndexOfChar },
            { M.Def(TInt32, "System.String.IndexOf", TString, TInt32), ResolverString.IndexOfString },
            { M.Def(TString, "System.String.Substring", TInt32), ResolverString.Substring },
            { M.Def(TString, "System.String.Substring", TInt32, TInt32), ResolverString.Substring },

            { M.Def(TVoid, "System.Array.Copy", TArray, TInt32, TArray, TInt32,TInt32, TBoolean), ResolverArray.Copy },

            { M.Def(TString, "System.Environment.GetResourceFromDefault", TString), ResolverEnvironment.GetResourceFromDefault },
            { M.Def(TString, "System.Environment.GetRuntimeResourceString", TString, ArrayOf(TObject)), ResolverEnvironment.GetRuntimeResourceString },

            { M.Def(TString, "System.ThrowHelper.GetResourceName", "System.ExceptionResource"), ResolverThrowHelper.GetResourceName },
            { M.Def(TString, "System.ThrowHelper.GetArgumentName", "System.ExceptionArgument"), ResolverThrowHelper.GetArgumentName },

            { M.Def(TBoolean, "System.Type.op_Equality", TType, TType), ResolverType.op_Equality },
            { M.Def(TBoolean, "System.Type.op_Inequality", TType, TType), ResolverType.op_Inequality },
            { M.Def(TType, "System.Type.GetTypeFromHandle", "System.RuntimeTypeHandle"), ResolverType.GetTypeFromHandle },

            { M.Def(TBoolean, "System.RuntimeTypeHandle.IsInterface", "System.RuntimeType"), ResolverSystem.RuntimeTypeHandle_IsInterface },

            { M.Def(TVoid, "System.Runtime.CompilerServices.RuntimeHelpers.InitializeArray", TArray, "System.RuntimeFieldHandle"), ResolverArray.InitializeArray },

        };

        public static Expr ResolveCall(ICall call) {
            var mRef = call.CallMethod;
            var mDef = mRef.Resolve();
            // A call that needs translating into a javascript call
            var m = new M(mDef);
            var fn = callMap.ValueOrDefault(m);
            if (fn != null) {
                var resolved = fn(call);
                return resolved;
            }
            // A call that is defined in JS only
            var type = mDef.DeclaringType;
            var jsClass = type.GetCustomAttribute<JsClassAttribute>();
            if (jsClass != null) {
                if (mDef.IsExternal()) {
                    var ctx = call.Ctx;
                    if (mDef.IsSetter || mDef.IsGetter) {
                        var propertyName = JsCase(mDef.Name.Substring(4));
                        if (mDef.IsStatic) {
                            propertyName = JsCase(mDef.DeclaringType.Name) + "." + propertyName;
                        }
                        return new ExprJsResolvedProperty(ctx, call.Type, call.Obj, propertyName);
                    } else {
                        var methodName = JsCase(mDef.Name);
                        if (mDef.IsStatic) {
                            methodName = JsCase(mDef.DeclaringType.Name) + "." + methodName;
                        }
                        return new ExprJsResolvedMethod(ctx, call.Type, call.Obj, methodName, call.Args);
                    }
                }
            }
            // No special resolution available
            return null;
        }

    }
}
