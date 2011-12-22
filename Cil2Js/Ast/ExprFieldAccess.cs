using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Ast {
    public class ExprFieldAccess : ExprVar {

        public ExprFieldAccess(Ctx ctx, Expr obj, FieldReference field)
            : base(ctx) {
            this.Obj = obj;
            this.Field = field;
            if (field.FieldType.IsGenericParameter) {
                var declType = field.DeclaringType;
                if (!declType.IsGenericInstance) {
                    throw new InvalidOperationException("Declaring type surely should be a generic instance?!?");
                }
                var genericInstanceDeclType = (GenericInstanceType)declType;
                var genericParameterFieldType = (GenericParameter)field.FieldType;
                this.type = genericInstanceDeclType.GenericArguments[genericParameterFieldType.Position];
            } else {
                this.type = field.FieldType;
            }
        }

        private TypeReference type;
        public Expr Obj { get; private set; }
        public FieldReference Field { get; private set; }

        public bool IsStatic {
            get { return this.Obj == null; }
        }

        public override Expr.NodeType ExprType {
            get { return NodeType.FieldAccess; }
        }

        public override TypeReference Type {
            get { return this.type; }
        }

        public override string ToString() {
            if (this.IsStatic) {
                return string.Format("{0}.{1}", this.Field.DeclaringType.Name, this.Field.Name);
            } else {
                return string.Format("{0}.{1}", this.Obj, this.Field.Name);
            }
        }

    }
}
