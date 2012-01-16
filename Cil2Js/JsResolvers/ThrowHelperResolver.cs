﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;

namespace Cil2Js.JsResolvers {
    static class ThrowHelperResolver {

        // TODO: These may not be required finally.
        // But the IL implementations use a large switch statement that cannot be converted to JS
        // because the 'default' case finishes with a branch to a different instruction than all the
        // othe cases. The switch sequencer cannot currently handle this.

        public static Expr GetResourceName(ICall call) {
            var ctx = call.Ctx;
            return new ExprLiteral(ctx, "", ctx.String);
        }

        public static Expr GetArgumentName(ICall call) {
            var ctx = call.Ctx;
            return new ExprLiteral(ctx, "", ctx.String);
        }

    }
}
