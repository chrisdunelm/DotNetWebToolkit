//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Mono.Cecil;

//namespace DotNetWebToolkit.Cil2Js.Ast {

//    public class StmtStoreObj : Stmt {

//        public StmtStoreObj(Ctx ctx, Expr destination, Expr source)
//            : base(ctx) {
//            this.Destination = destination;
//            this.Source = source;
//        }

//        public Expr Destination { get; private set; }
//        public Expr Source { get; private set; }

//        public override Stmt.NodeType StmtType {
//            get { return NodeType.StoreObj; }
//        }

//    }

//}
