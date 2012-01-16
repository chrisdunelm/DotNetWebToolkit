using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Output;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {
    public static partial class JsResolver {

        private static Dictionary<M, Func<ICall, Expr>> callMap = new Dictionary<M, Func<ICall, Expr>>(M.ValueEqComparer) {
            { M.Def(TVoid, "System.Action..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`1..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`2..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`3..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`4..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`5..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`6..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`7..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`8..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action.Invoke"), SystemResolver.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`1.Invoke", (string)null), SystemResolver.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`2.Invoke", (string)null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`3.Invoke", (string)null, null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`4.Invoke", (string)null, null, null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`5.Invoke", (string)null, null, null, null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`6.Invoke", (string)null, null, null, null, null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`7.Invoke", (string)null, null, null, null, null, null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`8.Invoke", (string)null, null, null, null, null, null, null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Func`1..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`2..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`3..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`4..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`5..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`6..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`7..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`8..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`9..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(null, "System.Func`1.Invoke"), SystemResolver.ActionFunc_Invoke },
            { M.Def(null, "System.Func`2.Invoke", (string)null), SystemResolver.ActionFunc_Invoke },
            { M.Def(null, "System.Func`3.Invoke", (string)null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(null, "System.Func`4.Invoke", (string)null, null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(null, "System.Func`5.Invoke", (string)null, null, null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(null, "System.Func`6.Invoke", (string)null, null, null, null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(null, "System.Func`7.Invoke", (string)null, null, null, null, null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(null, "System.Func`8.Invoke", (string)null, null, null, null, null, null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(null, "System.Func`9.Invoke", (string)null, null, null, null, null, null, null, null), SystemResolver.ActionFunc_Invoke },

            { M.Def(TBoolean, "System.String.op_Equality", TString, TString), StringResolver.op_Equality },
            { M.Def(TInt32, "System.String.get_Length"), StringResolver.get_Length },
            { M.Def(TChar, "System.String.get_Chars", TInt32), StringResolver.get_Chars },
            { M.Def(TString, "System.String.Concat", TString, TString), StringResolver.ConcatStrings },
            { M.Def(TString, "System.String.Concat", TString, TString, TString), StringResolver.ConcatStrings },
            { M.Def(TString, "System.String.Concat", TString, TString, TString, TString), StringResolver.ConcatStrings },
            { M.Def(TString, "System.String.Concat", ArrayOf(TString)), StringResolver.ConcatStringsMany },
            { M.Def(TInt32, "System.String.IndexOf", TChar), StringResolver.IndexOf },
            { M.Def(TInt32, "System.String.IndexOf", TString), StringResolver.IndexOf },
            { M.Def(TInt32, "System.String.IndexOf", TChar, TInt32), StringResolver.IndexOf },
            { M.Def(TInt32, "System.String.IndexOf", TString, TInt32), StringResolver.IndexOf },
            { M.Def(TString, "System.String.Substring", TInt32), StringResolver.Substring },
            { M.Def(TString, "System.String.Substring", TInt32, TInt32), StringResolver.Substring },

            { M.Def(TBoolean, "System.Object.Equals", TObject), SystemResolver.ObjectEquals },

            { M.Def(TVoid, "System.Array.Copy", TArray, TInt32, TArray, TInt32,TInt32, TBoolean), ArrayResolver.Copy },
            //{ M.Def(TVoid, "System.Array.Clear", TArray, TInt32, TInt32), ArrayResolver.Clear },

            { M.Def(TString, "System.Environment.GetResourceFromDefault", TString), EnvironmentResolver.GetResourceFromDefault },

            { M.Def(TString, "System.ThrowHelper.GetResourceName", "System.ExceptionResource"), ThrowHelperResolver.GetResourceName },
            { M.Def(TString, "System.ThrowHelper.GetArgumentName", "System.ExceptionArgument"), ThrowHelperResolver.GetArgumentName },
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
            var jsClass = type.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "Cil2Js.Attributes.JsClassAttribute");
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
