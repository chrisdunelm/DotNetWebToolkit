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
                this.catchType = s.catchType;
                this.Try = s.Try;
                this.Catches = s.Catches;
                this.Finally = s.Finally;
            }

            public Instruction @try { get; private set; }
            public Instruction @catch { get; private set; }
            public Instruction @finally { get; private set; }
            public TypeReference catchType { get; private set; }
            public Stmt Try { get; private set; }
            public IEnumerable<Catch> Catches { get; private set; }
            public Stmt Finally { get; private set; }

        }

        public class Catch {
            public Catch(Stmt stmt, ExprVar exceptionObject) {
                this.Stmt = stmt;
                this.ExceptionObject = exceptionObject;
            }
            public Stmt Stmt { get; private set; }
            public ExprVar ExceptionObject { get; private set; }
        }

        public StmtTry(Instruction @try, Instruction @catch, Instruction @finally, TypeReference catchType) {
            this.@try = @try;
            this.@catch = @catch;
            this.@finally = @finally;
            this.catchType = catchType;
        }

        public StmtTry(Stmt @try, IEnumerable<Catch> catches, Stmt @finally) {
            this.Try = @try;
            this.Catches = catches;
            this.Finally = @finally;
        }

        private Instruction @try, @catch, @finally;
        private TypeReference catchType;

        public Stmt Try { get; private set; }
        public IEnumerable<Catch> Catches { get; private set; }
        public Stmt Finally { get; private set; }

        void IInstructionMappable.Map(Dictionary<Instruction, List<Stmt>> map) {
            // Get the nested mapping, just inside this 'try' statement
            this.Try = map[this.@try].SkipWhile(x => x != this).Skip(1).First();
            if (this.@catch != null) {
                this.Catches = new[] { new Catch(map[this.@catch][0], new ExprVarLocal(this.catchType)) };
            } else {
                this.Catches = null;
            }
            if (this.@finally != null) {
                this.Finally = map[this.@finally][0];
            } else {
                this.Finally = null;
            }
        }

        public override Stmt.NodeType StmtType {
            get { return NodeType.Try; }
        }

    }
}
