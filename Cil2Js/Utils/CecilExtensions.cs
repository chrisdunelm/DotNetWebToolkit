using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Utils {
    public static class CecilExtensions {

        public static GenericInstanceType MakeGenericType(this TypeReference typeRef, params TypeReference[] args) {
            if (typeRef.GenericParameters.Count != args.Length) {
                throw new ArgumentException("Wrong number of generic arguments");
            }
            var ret = new GenericInstanceType(typeRef);
            foreach (var arg in args) {
                ret.GenericArguments.Add(arg);
            }
            return ret;
        }

        public static GenericInstanceMethod MakeGenericMethod(this MethodReference methodRef, params TypeReference[] args) {
            if (methodRef.GenericParameters.Count != args.Length) {
                throw new ArgumentException("Wrong number of generic arguments");
            }
            var ret = new GenericInstanceMethod(methodRef);
            foreach (var arg in args) {
                ret.GenericArguments.Add(arg);
            }
            return ret;
        }

        public static TypeReference GetBaseType(this TypeReference typeRef) {
            if (typeRef == null) {
                throw new ArgumentNullException("typeRef");
            }
            if (typeRef.IsDefinition) {
                return ((TypeDefinition)typeRef).BaseType;
            }
            if (typeRef.IsGenericInstance) {
                var typeGenInst = (GenericInstanceType)typeRef;
                var genericDef = typeGenInst.GetElementType();
                var baseType = genericDef.GetBaseType();
                if (baseType.IsGenericInstance) {
                    var baseTypeGenInst = (GenericInstanceType)baseType;
                    var baseTypeWithGenArgs = new GenericInstanceType(baseType.GetElementType());
                    foreach (var genArg in baseTypeGenInst.GenericArguments) {
                        if (genArg.IsGenericParameter) {
                            var genArgGenParam = (GenericParameter)genArg;
                            var arg = typeGenInst.GenericArguments[genArgGenParam.Position];
                            baseTypeWithGenArgs.GenericArguments.Add(arg);
                        } else {
                            baseTypeWithGenArgs.GenericArguments.Add(genArg);
                        }
                    }
                    return baseTypeWithGenArgs;
                } else {
                    return baseType;
                }
            }
            return typeRef.Resolve().BaseType;
        }

        public static IEnumerable<MethodReference> GetMethods(this TypeReference tRef) {
            // TODO: Won't work with generics
            var tDef = tRef.Resolve();
            return tDef.Methods;
        }

        //public static IEnumerable<MethodReference> GetMethods(this TypeReference typeRef) {
        //    var typeDef = typeRef.Resolve();
        //    foreach (var m in typeDef.Methods) {
        //        //if (m.HasGenericParameters()) {
        //            if (m.HasGenericParameters) {
        //                throw new NotImplementedException();
        //            } else {
        //                var returnType = m.GetResolvedReturnType((IGenericInstance)typeRef);
        //                var mGen = new MethodDefinition(m.Name, m.Attributes, returnType);
        //                ((MethodReference)mGen).DeclaringType = typeRef;
        //                foreach (var p in m.Parameters) {
        //                    if (p.ParameterType.IsGenericParameter) {
        //                        var pp = new ParameterDefinition(p.Name, p.Attributes, p.GetResolvedType(mGen));
        //                        mGen.Parameters.Add(pp);
        //                    } else {
        //                        mGen.Parameters.Add(p);
        //                    }
        //                }
        //                yield return mGen;
        //            }
        //        //} else {
        //        //    yield return m;
        //        //}
        //    }
        //}

        //public static MethodReference GetBaseMethodByType(this MethodReference methodRef) {
        //    var methodDef = methodRef.Resolve();
        //    if (!methodDef.IsVirtual) {
        //        throw new ArgumentException("Method must be virtual");
        //    }
        //    if (methodDef.IsNewSlot) {
        //        // No base method, this is the base method
        //        return null;
        //    }
        //    var baseTypeRef = methodRef.DeclaringType.GetBaseType();
        //    var baseTypeDef = baseTypeRef.Resolve();
        //    foreach (var baseMethod in baseTypeDef.Methods) {
        //        if (baseMethod.MethodMatch(methodDef)) {
        //            //if (baseMethod.Parameters.Any(x => x.ParameterType.IsGenericParameter) || baseMethod.ReturnType.IsGenericParameter) {

        //            //    var mRef = new MethodReference(baseMethod.Name);
        //            //    return mRef;
        //            //}
        //            return baseMethod;
        //            //if (baseTypeRef.IsGenericInstance) {
        //            //    var baseMethodRef = new MethodReference(baseMethod.Name, methodRef.GetResolvedReturnType(), baseTypeRef);
        //            //    for (int i = 0; i < baseMethod.Parameters.Count; i++) {
        //            //        var pType = baseMethod.Parameters[i].GetResolvedType(baseMethodRef);
        //            //    }
        //            //    var ret = new GenericInstanceMethod(baseMethodRef);
        //            //    return ret;
        //            //} else {
        //            //    return baseMethod;
        //            //}
        //        }
        //    }
        //    throw new InvalidOperationException("Base method cannot be found - this should not occur");
        //}

        public static MethodReference GetBasemostMethodByType(this MethodReference methodRef) {
            throw new NotImplementedException();
        }

    }
}
