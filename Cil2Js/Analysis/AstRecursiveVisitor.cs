using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    public abstract class AstRecursiveVisitor : AstVisitor {

        private bool first = true;
        private List<StmtContinuation> continuations = new List<StmtContinuation>();
        private Queue<ICode> todo = new Queue<ICode>();
        private Dictionary<ICode, ICode> map = new Dictionary<ICode, ICode>();
        private HashSet<ICode> seen = new HashSet<ICode>();

        protected virtual ICode TopLevel(ICode c) {
            this.seen.Add(c);
            this.todo.Enqueue(c);
            ICode ret = null;
            while (todo.Any()) {
                var node = todo.Dequeue();
                this.BlockStart(node);
                var transformed = this.Visit(node);
                this.BlockEnd(node, transformed);
                if (ret == null) {
                    ret = transformed;
                }
            }
            bool anyChanges = false;
            foreach (var continuation in this.continuations) {
                var mapped = this.map.ValueOrDefault(continuation.To);
                if (mapped != null) {
                    continuation.To = (Stmt)mapped;
                    anyChanges = true;
                }
            }
            if (anyChanges) {
                // Cloned to make sure the returned object will be different from ICode argument
                // Other code relies on it being different if this visitor has made changes to the AST
                ret = (ICode)ret.Clone();
            }
            return ret;
        }

        protected virtual void BlockStart(ICode block) { }
        protected virtual void BlockEnd(ICode originalBlock, ICode transformedBlock) { }

        protected override ICode VisitContinuation(StmtContinuation s) {
            this.continuations.Add(s);
            if (this.seen.Add(s.To)) {
                this.todo.Enqueue(s.To);
            }
            return s;
        }

        public override ICode Visit(ICode c) {
            if (this.first) {
                this.first = false;
                return this.TopLevel(c);
            } else {
                var ret = base.Visit(c);
                if (c != ret) {
                    if (c.CodeType == CodeType.Statement) {
                        this.map.Add(c, ret);
                    }
                }
                return ret;
            }
        }

    }
}
