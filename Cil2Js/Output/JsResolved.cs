using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;

namespace Cil2Js.Output {

    public enum JsResolvedType {
        Method,
        Property,
        /// <summary>
        /// The call is resolved by providing a new Expr to replace the call expression.
        /// </summary>
        Expr,
    }

    public abstract class JsResolved {

        public abstract JsResolvedType Type { get; }

    }

    public class JsResolvedMethod : JsResolved {

        public JsResolvedMethod(Expr obj, string methodName, params Expr[] args) : this(obj, methodName, (IEnumerable<Expr>)args) { }

        public JsResolvedMethod(Expr obj, string methodName, IEnumerable<Expr> args) {
            this.Obj = obj;
            this.MethodName = methodName;
            this.Args = args;
        }

        public Expr Obj { get; private set; }
        public string MethodName { get; private set; }
        public IEnumerable<Expr> Args { get; private set; }

        public override JsResolvedType Type {
            get { return JsResolvedType.Method; }
        }

    }

    public class JsResolvedProperty : JsResolved {

        public JsResolvedProperty(Expr obj, string propertyName) {
            this.Obj = obj;
            this.PropertyName = propertyName;
        }

        public Expr Obj { get; private set; }
        public string PropertyName { get; private set; }

        public override JsResolvedType Type {
            get { return JsResolvedType.Property; }
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
