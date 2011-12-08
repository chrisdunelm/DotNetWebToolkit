using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using Cil2Js.Analysis;
using System.Diagnostics;
using Mono.Cecil;

namespace Cil2Js.Ast {

    [DebuggerTypeProxy(typeof(DebugView))]
    public class StmtTry : Stmt, IInstructionMappable {

        class DebugView {

            public DebugView(StmtTry s) {
                this.@try = s.@try;
                this.@catch = s.@catch;
                this.@finally = s.@finally;
                this.Try = s.Try;
                this.Catch = s.Catch;
                this.Finally = s.Finally;
                this.CatchType = s.CatchType;
            }

            public Instruction @try { get; private set; }
            public Instruction @catch { get; private set; }
            public Instruction @finally { get; private set; }
            public Stmt Try { get; private set; }
            public Stmt Catch { get; private set; }
            public Stmt Finally { get; private set; }
            public TypeReference CatchType { get; private set; }

        }

        public StmtTry(Instruction @try, Instruction @catch, Instruction @finally, TypeReference catchType) {
            this.@try = @try;
            this.@catch = @catch;
            this.@finally = @finally;
            this.CatchType = catchType;
        }

        public StmtTry(Stmt @try, Stmt @catch, Stmt @finally, TypeReference catchType) {
            this.Try = @try;
            this.Catch = @catch;
            this.Finally = @finally;
            this.CatchType = catchType;
        }

        private Instruction @try, @catch, @finally;
        public Stmt Try { get; private set; }
        public Stmt Catch { get; private set; }
        public Stmt Finally { get; private set; }
        public TypeReference CatchType { get; private set; }

        void IInstructionMappable.Map(Dictionary<Instruction, List<Stmt>> map) {
            if (this.@try != null) {
                // Get the nested mapping, just inside this 'try' statement
                this.Try = map[this.@try].SkipWhile(x => x != this).Skip(1).First();
            }
            if (this.@catch != null) {
                this.Catch = map[this.@catch][0];
            }
            if (this.@finally != null) {
                this.Finally = map[this.@finally][0];
            }
        }

        public override Stmt.NodeType StmtType {
            get { return NodeType.Try; }
        }

    }
}
