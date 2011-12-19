using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Ast {
    public class Ctx {

        public Ctx(MethodDefinition method) {
            this.Method = method;
            this.TypeSystem = method.Module.TypeSystem;
            this.ExprGen = Expr.CreateExprGen(this);
        }

        public MethodDefinition Method { get; private set; }
        public TypeSystem TypeSystem { get; private set; }

        public Expr.Gen ExprGen { get; private set; }

        public TypeReference Object { get { return this.TypeSystem.Object; } }
        public TypeReference Int32 { get { return this.TypeSystem.Int32; } }
        public TypeReference Boolean { get { return this.TypeSystem.Boolean; } }
        public TypeReference Char { get { return this.TypeSystem.Char; } }
        public TypeReference String { get { return this.TypeSystem.String; } }

    }
}
