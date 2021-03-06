﻿using System;
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
            return string.Format("[{0}:{1}]", this.StartStackSize, this.EndStackSize) +
                Environment.NewLine +
                string.Join(Environment.NewLine, this.Insts.Select(x => x.ToString()));
        }

    }
}
