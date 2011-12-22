using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Utils {
    public static class GenericTypeExtensions {

        public static TypeReference GetResolvedType(this ParameterDefinition parameter, MethodReference methodRef) {
            var type = parameter.ParameterType;
            if (type.IsGenericParameter) {
                IGenericInstance genericInstance;
                switch (type.MetadataType) {
                case MetadataType.Var: genericInstance = (IGenericInstance)methodRef.DeclaringType; break;
                case MetadataType.MVar: genericInstance = (IGenericInstance)methodRef; break;
                default: throw new InvalidOperationException("Must be Var or MVar here");
                }
                return genericInstance.GenericArguments[((GenericParameter)type).Position];
            } else {
                return type;
            }
        }

        public static TypeReference GetResolvedType(this FieldReference field) {
            var type = field.FieldType;
            if (type.IsGenericParameter) {
                return ((GenericInstanceType)field.DeclaringType).GenericArguments[((GenericParameter)type).Position];
            } else {
                return type;
            }
        }

        public static TypeReference GetResolvedReturnType(this MethodReference methodRef) {
            var type = methodRef.ReturnType;
            if (type.IsGenericParameter) {
                IGenericInstance genericInstance;
                switch (type.MetadataType) {
                case MetadataType.Var: genericInstance = (IGenericInstance)methodRef.DeclaringType; break;
                case MetadataType.MVar: genericInstance = (IGenericInstance)methodRef; break;
                default: throw new InvalidOperationException("Must be Var or MVar here");
                }
                return genericInstance.GenericArguments[((GenericParameter)type).Position];
            } else {
                return type;
            }
        }

    }
}
