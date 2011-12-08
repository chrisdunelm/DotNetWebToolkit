using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil.Cil;
using Mono.Cecil;
using System.Diagnostics;

namespace Cil2Js.Analysis {

    [DebuggerTypeProxy(typeof(DebugView))]
    public class StmtCil : Stmt {

        class DebugView {

            public DebugView(StmtCil s) {
                this.Method = s.Method;
                this.Insts = s.Insts;
                this.EndCil = s.EndCil;
                this.StartStackSize = s.StartStackSize;
                this.EndStackSize = s.EndStackSize;
            }

            public MethodDefinition Method { get; private set; }
            public IEnumerable<Instruction> Insts { get; private set; }
            public Stmt EndCil { get; private set; }
            public int StartStackSize { get; set; }
            public int EndStackSize { get; set; }

        }

        public StmtCil(MethodDefinition method, IEnumerable<Instruction> insts, Stmt endCil) {
            this.Method = method;
            this.Insts = insts;
            this.EndCil = endCil;
            this.StartStackSize = 0;
            this.EndStackSize = 0;
        }

        public MethodDefinition Method { get; private set; }
        public IEnumerable<Instruction> Insts { get; private set; }
        public Stmt EndCil { get; private set; }
        public int StartStackSize { get; set; }
        public int EndStackSize { get; set; }

        public override Stmt.NodeType StmtType {
            get { return NodeType.Cil; }
        }

        public override string ToString() {
            int l = this.Insts.Count();
            if (l == 1) {
                return this.Insts.First().ToString();
            } else {
                return string.Format("{0} - {1}{2}{3}",
                    this.Insts.First(),
                    string.Join(", ", this.Insts.Skip(1).Take(Math.Max(0, l - 2)).Select(x => x.OpCode.Code)),
                    l > 2 ? " - " : "",
                    this.Insts.Last());
            }
        }

    }
}
