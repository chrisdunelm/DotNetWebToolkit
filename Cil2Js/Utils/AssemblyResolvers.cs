using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Utils {
    static class AssemblyResolvers {

        private static List<string> paths = new List<string>();

        public static void AddDirectory(string path) {
            paths.Add(path);
        }

        private static ReaderParameters readerParameters = null;
        public static ReaderParameters ReaderParameters {
            get {
                if (readerParameters == null) {
                    var re = new DefaultAssemblyResolver();
                    foreach (var path in paths) {
                        re.AddSearchDirectory(path);
                    }
                    readerParameters = new ReaderParameters {
                        AssemblyResolver = re
                    };
                }
                return readerParameters;
            }
        }

    }
}
