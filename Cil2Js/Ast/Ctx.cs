using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Ast {
    public class Ctx {

        public Ctx(TypeReference tRef, MethodReference mRef) {
            this.TRef = tRef;
            this.TDef = tRef.Resolve();
            this.MRef = mRef;
            this.MDef = mRef.Resolve();
            this.TypeSystem = mRef.Module.TypeSystem;
            this.ExprGen = Expr.CreateExprGen(this);
            this.This = this.MDef.IsStatic ? null : new ExprVarThis(this, tRef);
        }

        public TypeReference TRef { get; private set; }
        public TypeDefinition TDef { get; private set; }
        public MethodReference MRef { get; private set; }
        public MethodDefinition MDef { get; private set; }

        public ExprVarThis This { get; private set; }

        public TypeSystem TypeSystem { get; private set; }

        public Expr.Gen ExprGen { get; private set; }

        public TypeReference Void { get { return this.TypeSystem.Void; } }
        public TypeReference Object { get { return this.TypeSystem.Object; } }
        public TypeReference Int32 { get { return this.TypeSystem.Int32; } }
        public TypeReference Boolean { get { return this.TypeSystem.Boolean; } }
        public TypeReference Char { get { return this.TypeSystem.Char; } }
        public TypeReference String { get { return this.TypeSystem.String; } }
        public TypeReference IntPtr { get { return this.TypeSystem.IntPtr; } }

    }
}
