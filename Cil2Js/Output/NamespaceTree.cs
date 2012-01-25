using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Utils;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Output {
    class NamespaceTree {

        private static IEnumerable<NamespaceTree> Make(IEnumerable<Tuple<string[], TypeReference>> state) {
            var byRootNs = state.GroupBy(x => x.Item1[0]);
            return byRootNs.Select(ns => {
                var subNss = Make(ns.Where(x => x.Item1.Length > 1).Select(x => Tuple.Create(x.Item1.Skip(1).ToArray(), x.Item2)));
                return new NamespaceTree {
                    NamespacePart = ns.Key,
                    Namespaces = subNss,
                    Types = ns.Where(x => x.Item1.Length == 1).Select(x => x.Item2).ToArray()
                };
            }).ToArray();
        }

        public static IEnumerable<NamespaceTree> Make(IEnumerable<TypeReference> types) {
            var splitNs = types.Select(x => Tuple.Create(x.Namespace.Split('.'), x)).ToArray();
            return Make(splitNs);
        }

        public string NamespacePart { get; private set; }
        public IEnumerable<NamespaceTree> Namespaces { get; private set; }
        public IEnumerable<TypeReference> Types { get; private set; }

    }
}
