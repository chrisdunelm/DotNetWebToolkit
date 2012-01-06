using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Utils {
    public static class GenericTypeExtensions {

        public static TypeReference GetResolvedType(this ParameterDefinition parameter, TypeReference tRef, MethodReference mRef) {
            var type = parameter.ParameterType;
            if (type.IsGenericParameter) {
                IGenericInstance genericInstance;
                switch (type.MetadataType) {
                case MetadataType.Var: genericInstance = (IGenericInstance)tRef; break;
                case MetadataType.MVar: genericInstance = (IGenericInstance)mRef; break;
                default: throw new InvalidOperationException("Metadata must be Var or MVar");
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

        public static TypeReference GetResolvedReturnType(this MethodReference mRef, TypeReference tRef) {
            var type = mRef.ReturnType;
            if (type.IsGenericParameter) {
                IGenericInstance genericInstance;
                switch (type.MetadataType) {
                case MetadataType.Var: genericInstance = (IGenericInstance)tRef; break;
                case MetadataType.MVar: genericInstance = (IGenericInstance)mRef; break;
                default: throw new InvalidOperationException("Metadata must be Var or MVar");
                }
                return genericInstance.GenericArguments[((GenericParameter)type).Position];
            } else {
                return type;
            }
        }

    }
}
