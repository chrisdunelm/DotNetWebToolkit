using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class Ctx {

        public Ctx(TypeReference tRef, MethodReference mRef) {
            this.TRef = tRef;
            this.TDef = tRef.Resolve();
            this.MRef = mRef;
            this.MDef = mRef.Resolve();
            this.Module = mRef.Module;
            this.TypeSystem = mRef.Module.TypeSystem;
            this.ExprGen = Expr.CreateExprGen(this);
            this.This = this.MDef.IsStatic ? null : new ExprVarThis(this, tRef);
            this.type = new Lazy<TypeReference>(() => this.Module.Import(typeof(Type)));
        }

        public TypeReference TRef { get; private set; }
        public TypeDefinition TDef { get; private set; }
        public MethodReference MRef { get; private set; }
        public MethodDefinition MDef { get; private set; }

        public ExprVarThis This { get; private set; }

        public ModuleDefinition Module { get; private set; }
        public TypeSystem TypeSystem { get; private set; }

        public Expr.Gen ExprGen { get; private set; }

        public TypeReference Void { get { return this.TypeSystem.Void; } }
        public TypeReference Boolean { get { return this.TypeSystem.Boolean; } }
        public TypeReference Char { get { return this.TypeSystem.Char; } }
        public TypeReference SByte { get { return this.TypeSystem.SByte; } }
        public TypeReference Byte { get { return this.TypeSystem.Byte; } }
        public TypeReference Int16 { get { return this.TypeSystem.Int16; } }
        public TypeReference Int32 { get { return this.TypeSystem.Int32; } }
        public TypeReference Int64 { get { return this.TypeSystem.Int64; } }
        public TypeReference UInt16 { get { return this.TypeSystem.UInt16; } }
        public TypeReference UInt32 { get { return this.TypeSystem.UInt32; } }
        public TypeReference UInt64 { get { return this.TypeSystem.UInt64; } }
        public TypeReference Single { get { return this.TypeSystem.Single; } }
        public TypeReference Double { get { return this.TypeSystem.Double; } }
        public TypeReference Object { get { return this.TypeSystem.Object; } }
        public TypeReference String { get { return this.TypeSystem.String; } }
        public TypeReference IntPtr { get { return this.TypeSystem.IntPtr; } }

        private Lazy<TypeReference> type;
        public TypeReference Type { get { return this.type.Value; } }

        public ExprVarParameter MethodParameter(int index) {
            return new ExprVarParameter(this, this.MRef.Parameters[index]);
        }

    }
}
