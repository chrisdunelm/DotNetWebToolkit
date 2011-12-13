using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;

namespace Cil2Js.Output {

    public enum JsResolvedType {
        /// <summary>
        /// The call is resolved by giving the JS name of the function being called.
        /// </summary>
        Name,
        ///// <summary>
        ///// The call will be resolved to a direct function, but the name is not yet known.
        ///// </summary>
        //NameRef,
        /// <summary>
        /// The call is resolved by providing a new Expr to replace the call expression.
        /// </summary>
        Expr,
    }

    public abstract class JsResolved {

        public abstract JsResolvedType Type { get; }

    }

    public class JsResolvedName : JsResolved {

        public JsResolvedName(string name) {
            this.Name = name;
        }

        public string Name { get; private set; }

        public override JsResolvedType Type {
            get { return JsResolvedType.Name; }
        }

    }

    //public class JsResolvedNameRef : JsResolved {

    //    public JsResolvedNameRef(NameRef nameRef) {
    //        this.NameRef = nameRef;
    //    }

    //    public NameRef NameRef { get; private set; }

    //    public override JsResolvedType Type {
    //        get { return JsResolvedType.NameRef; }
    //    }

    //}

    public class JsResolvedExpr : JsResolved {

        public JsResolvedExpr(Expr expr) {
            this.Expr = expr;
        }

        public Expr Expr { get; private set; }

        public override JsResolvedType Type {
            get { return JsResolvedType.Expr; }
        }

    }

}
