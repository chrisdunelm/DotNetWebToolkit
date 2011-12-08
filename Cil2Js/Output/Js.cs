using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Cil2Js.Analysis;
using Cil2Js.Ast;

namespace Cil2Js.Output {
    public class Js {

        public static string CreateFrom(MethodDefinition method) {
            return CreateFrom(new[] { method });
        }

        public static string CreateFrom(IEnumerable<MethodDefinition> rootMethods) {
            var todo = new Queue<MethodDefinition>();
            foreach (var method in rootMethods) {
                todo.Enqueue(method);
            }
            var seen = new HashSet<MethodDefinition>(rootMethods);
            var asts = new Dictionary<MethodDefinition, ICode>();

            while (todo.Any()) {
                var method = todo.Dequeue();
                var ast = Transcoder.ToAst(method);
                asts.Add(method, ast);
                var calls = VisitorFindCalls.V(ast);
                foreach (var call in calls) {
                    var calledMethod = call.Resolve();
                    if (seen.Add(calledMethod)) {
                        todo.Enqueue(calledMethod);
                    }
                }
            }

            var js = new StringBuilder();
            foreach (var kv in asts) {
                var method = kv.Key;
                var ast = kv.Value;
                var s = JsMethod.Create(method, method.Name, null, ast);
                js.Append(s);
                js.AppendLine();
            }

            return js.ToString();
        }

    }
}
