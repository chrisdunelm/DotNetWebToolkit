using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using Cil2Js.Ast;

namespace Cil2Js.Analysis {
    interface IInstructionMappable {

        void Map(Dictionary<Instruction, List<Stmt>> map);

    }
}
