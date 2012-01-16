using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil.Cil;
using Mono.Cecil;
using System.Diagnostics;

namespace DotNetWebToolkit.Cil2Js.Analysis {

    [DebuggerTypeProxy(typeof(DebugView))]
    public class StmtCil : Stmt {

        class DebugView {

            public DebugView(StmtCil s) {
                this.Method = s.Ctx.MRef;
                this.Insts = s.Insts;
                this.EndCil = s.EndCil;
                this.StartStackSize = s.StartStackSize;
                this.EndStackSize = s.EndStackSize;
            }

            public MethodReference Method { get; private set; }
            public IEnumerable<Instruction> Insts { get; private set; }
            public Stmt EndCil { get; private set; }
            public int StartStackSize { get; set; }
            public int EndStackSize { get; set; }

        }

        public enum SpecialBlock {
            Normal = 0,
            Start,
            End
        }

        public StmtCil(Ctx ctx, IEnumerable<Instruction> insts, Stmt endCil, SpecialBlock blockType = SpecialBlock.Normal)
            : base(ctx) {
            this.Insts = insts;
            this.BlockType = blockType;
            this.EndCil = endCil;
            this.StartStackSize = 0;
            this.EndStackSize = 0;
        }

        public IEnumerable<Instruction> Insts { get; private set; }
        public SpecialBlock BlockType { get; private set; }
        public Stmt EndCil { get; private set; }
        public int StartStackSize { get; set; }
        public int EndStackSize { get; set; }

        public override Stmt.NodeType StmtType {
            get { return NodeType.Cil; }
        }

        public override string ToString() {
            if (this.Insts == null) {
                return "<null>";
            }
            int l = this.Insts.Count();
            if (l == 0) {
                return "<empty>";
            }
            if (l == 1) {
                return string.Format("[{0}:{1}]{2}", this.StartStackSize, this.EndStackSize, this.Insts.First().ToString());
            } else {
                return string.Format("[{4}:{5}]{0} - {1}{2}{3}",
                    this.Insts.First(),
                    string.Join(", ", this.Insts.Skip(1).Take(Math.Max(0, l - 2)).Select(x => x.OpCode.Code)),
                    l > 2 ? " - " : "",
                    this.Insts.Last(),
                    this.StartStackSize, this.EndStackSize);
            }
        }

    }
}
