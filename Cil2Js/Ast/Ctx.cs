using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.JsResolvers.Classes;
using DotNetWebToolkit.Cil2Js.Output;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class Ctx {

        public Ctx(TypeReference tRef, MethodReference mRef, Ctx fromCtx = null) {
            this.TRef = tRef;
            this.TDef = tRef.Resolve();
            this.MRef = mRef;
            this.MDef = mRef.Resolve();
            this.Module = mRef.Module;
            this.TypeSystem = mRef.Module.TypeSystem;
            this.ExprGen = Expr.CreateExprGen(this);
            this.This = fromCtx == null ? (this.MDef.IsStatic ? null : new ExprVarThis(this, tRef)) : fromCtx.This;
            this.type = new Lazy<TypeReference>(() => this.Module.Import(typeof(Type)));
            this._int64 = new Lazy<TypeReference>(() => this.Module.Import(typeof(_Int64)));
            this._uint64 = new Lazy<TypeReference>(() => this.Module.Import(typeof(_UInt64)));
        }

        public TypeReference TRef { get; private set; }
        public TypeDefinition TDef { get; private set; }
        public MethodReference MRef { get; private set; }
        public MethodDefinition MDef { get; private set; }

        public ExprVarThis This { get; private set; }
        public NamedExpr ThisNamed { get { return this.This.NullThru(x => x.Named("this")); } }

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
        public TypeReference UIntPtr { get { return this.TypeSystem.UIntPtr; } }

        private Lazy<TypeReference> type;
        public TypeReference Type { get { return this.type.Value; } }
        private Lazy<TypeReference> _int64;
        public TypeReference _Int64 { get { return this._int64.Value; } }
        private Lazy<TypeReference> _uint64;
        public TypeReference _UInt64 { get { return this._uint64.Value; } }

        public ExprVarParameter MethodParameter(int index) {
            return new ExprVarParameter(this, this.MRef.Parameters[index]);
        }

        public NamedExpr MethodParameter(int index, string name) {
            return this.MethodParameter(index).Named(name);
        }

        public NamedExpr this[int index] {
            get {
                return this.MethodParameter(index, new string((char)('a' + index), 1));
            }
        }

        public ExprVarLocal Local(TypeReference type) {
            return new ExprVarLocal(this, type);
        }

        public NamedExpr Local(TypeReference type, string name) {
            return this.Local(type).Named(name);
        }

        public ExprLiteral Literal(object value, TypeReference type) {
            return new ExprLiteral(this, value, type);
        }

        public NamedExpr Literal(object value, TypeReference type, string name) {
            return this.Literal(value, type).Named(name);
        }

        public ExprLiteral Literal(bool value) {
            return this.Literal(value, this.Boolean);
        }

        public NamedExpr Literal(bool value, string name) {
            return this.Literal(value).Named(name);
        }

        public ExprLiteral Literal(int value) {
            return this.Literal(value, this.Int32);
        }

        public NamedExpr Literal(int value, string name) {
            return this.Literal(value).Named(name);
        }

        public ExprLiteral Literal(string value) {
            return this.Literal(value, this.String);
        }

        public NamedExpr Literal(string value, string name) {
            return this.Literal(value).Named(name);
        }

    }
}
