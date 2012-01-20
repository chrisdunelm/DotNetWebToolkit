using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;
using System.Reflection;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {
    public static partial class JsResolver {

        private static Dictionary<M, Func<Ctx, List<TypeReference>, Stmt>> methodMap = new Dictionary<M,Func<Ctx,List<TypeReference>,Stmt>>(M.ValueEqComparer) {
            { M.Def(TVoid, "System.Object..ctor"), ResolverSystem.Object_Ctor },
            { M.Def(TBoolean, "System.Object.Equals", TObject), ResolverSystem.Object_Equals },
            { M.Def(TType, "System.Object.GetType"), ResolverSystem.Object_GetType },
            { M.Def(TInt32, "System.Object.GetHashCode"), ResolverSystem.Object_GetHashCode },

            { M.Def(TVoid, "System.IntPtr..ctor", TInt32), ResolverSystem.IntPtrCtor },
            { M.Def(TBoolean, "System.IntPtr.Equals", TObject), ResolverSystem.TrivialBoxedValueType_Equals },

            { M.Def(TBoolean, "System.Boolean.Equals", TObject), ResolverSystem.TrivialBoxedValueType_Equals },

            { M.Def(TVoid, "System.Array.Clear", TArray, TInt32, TInt32), ResolverArray.Clear },
            
            { M.Def(TVoid, "System.RuntimeType..cctor"), ResolverType.cctor },
            { M.Def(TString, "System.RuntimeType.get_FullName"), ResolverType.get_FullName },
            { M.Def(TString, "System.RuntimeType.ToString"), ResolverType.ToString },
            
            { M.Def(TBoolean, "System.Int32.Equals", TObject), ResolverSystem.TrivialBoxedValueType_Equals },
            { M.Def(TBoolean, "System.Int32.Equals", TInt32), ResolverSystem.TrivialValueType_Equals },
            { M.Def(TInt32, "System.Int32.GetHashCode"), ResolverSystem.Int32_GetHashCode },
            
            { M.Def(TBoolean, "System.String.Equals", TObject), ResolverSystem.Object_Equals },

            // TODO: This is rubbish - must make it possible to match all generic arguments
            { M.Def(null, "System.Collections.Generic.EqualityComparer`1<System.Int32>.CreateComparer"), ResolverCollections.EqualityComparer_CreateComparer },

        };

        public static Stmt ResolveMethod(Ctx ctx, List<TypeReference> newTypesSeen) {
            // Explicit mapping
            var m = new M(ctx.MRef);
            var fn = methodMap.ValueOrDefault(m);
            if (fn != null) {
                var resolved = fn(ctx, newTypesSeen);
                return resolved;
            }
            // Attribute for internal function
            var jsAttr = ctx.MDef.GetCustomAttribute<JsAttribute>();
            if (jsAttr != null) {
                var implType = (TypeDefinition)jsAttr.ConstructorArguments[0].Value;
                var t = typeof(JsResolver).Module.ResolveType(implType.MetadataToken.ToInt32());
                var impl = (IJsImpl)Activator.CreateInstance(t);
                var stmt = impl.GetImpl(ctx);
                return stmt;
            }
            return null;
        }

    }
}
