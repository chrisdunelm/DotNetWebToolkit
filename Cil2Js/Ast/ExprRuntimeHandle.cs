using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class ExprRuntimeHandle : Expr {

        public ExprRuntimeHandle(Ctx ctx, MemberReference member)
            : base(ctx) {
            var tokenType = member.MetadataToken.TokenType;
            switch (tokenType) {
            case TokenType.TypeDef:
            case TokenType.TypeRef:
            case TokenType.TypeSpec:
            case TokenType.GenericParam:
                this.Member = ((TypeReference)member).FullResolve(ctx);
                this.type = ctx.Module.Import(typeof(RuntimeTypeHandle));
                break;
            case TokenType.Method:
            case TokenType.MethodSpec:
                this.Member = ((MethodReference)member).FullResolve(ctx);
                this.type = ctx.Module.Import(typeof(RuntimeMethodHandle));
                break;
            case TokenType.Field:
                this.Member = ((FieldReference)member).FullResolve(ctx);
                this.type = ctx.Module.Import(typeof(RuntimeFieldHandle));
                break;
            default:
                throw new NotImplementedException("Cannot handle token type: " + tokenType);
            }
        }

        public MemberReference Member { get; private set; }
        private TypeReference type;

        public override Expr.NodeType ExprType {
            get { return NodeType.RuntimeHandle; }
        }

        public override TypeReference Type {
            get { return this.type; }
        }

    }
}
