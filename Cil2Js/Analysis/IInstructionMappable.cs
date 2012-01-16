using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using DotNetWebToolkit.Cil2Js.Ast;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    interface IInstructionMappable {

        void Map(Dictionary<Instruction, List<Stmt>> map);

    }
}
