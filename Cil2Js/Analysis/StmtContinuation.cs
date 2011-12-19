using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil.Cil;
using Cil2Js.Utils;
using System.Diagnostics;

namespace Cil2Js.Analysis {

    [DebuggerTypeProxy(typeof(DebugView))]
    public class StmtContinuation : Stmt, IInstructionMappable {

        class DebugView {

            public DebugView(StmtContinuation s) {
                this.to = s.to;
                this.To = s.To;
            }

            public Instruction to { get; private set; }
            public Stmt To { get; private set; }

        }

        public StmtContinuation(Ctx ctx, Instruction to, bool leaveProtectedRegion)
            : base(ctx) {
            this.to = to;
            this.LeaveProtectedRegion = leaveProtectedRegion;
        }

        public StmtContinuation(Ctx ctx, Stmt to, bool leaveProtectedRegion)
            : base(ctx) {
            this.To = to;
            this.LeaveProtectedRegion = leaveProtectedRegion;
        }

        private Instruction to;
        public Stmt To { get; set; }
        public bool LeaveProtectedRegion { get; private set; }

        void IInstructionMappable.Map(Dictionary<Instruction, List<Stmt>> map) {
            if (this.to != null) {
                this.To = map[this.to][0];
            }
        }

        public override Stmt.NodeType StmtType {
            get { return NodeType.Continuation; }
        }

        public override string ToString() {
            if (this.to != null) {
                return string.Format("-> {0}{1}", this.to.ToString(), this.LeaveProtectedRegion ? " (leave protected region)" : "");
            } else {
                return string.Format("-> {0}{1}", this.To.ToString(), this.LeaveProtectedRegion ? " (leave protected region)" : "");
            }
        }

    }
}
