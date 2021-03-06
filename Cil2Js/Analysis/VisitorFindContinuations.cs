﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    public class VisitorFindContinuations : AstVisitor {

        public static bool Any(ICode root) {
            var v = new VisitorFindContinuations();
            v.Visit(root);
            return v.Continuations.Any();
        }

        public static IEnumerable<StmtContinuation> Get(ICode root) {
            var v = new VisitorFindContinuations();
            v.Visit(root);
            return v.Continuations;
        }

        private List<StmtContinuation> continuations = new List<StmtContinuation>();
        public IEnumerable<StmtContinuation> Continuations { get { return this.continuations; } }

        protected override ICode VisitContinuation(StmtContinuation s) {
            this.continuations.Add(s);
            return s;
        }

    }
}
