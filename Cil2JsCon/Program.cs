using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js;
using Mono.Options;

namespace Cil2JsCon {

    class Program {

        static void Usage() {
            Console.WriteLine("Cil2JsCon usage:");
            Console.WriteLine();
            Console.WriteLine("Cil2JsCon -in <input dll> -out <output js file>");
            Console.WriteLine();
        }

        static int Main(string[] args) {

            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            bool verbose = false;
            string inFilename = null;
            string outFilename = null;

            var p = new OptionSet {
                { "v", "Verbose", v => verbose = v != null },
                { "in=", "Input dll file", s => inFilename = s },
                { "out=", "Output JavaScript file. Will be overwritten if already exists", s => outFilename = s },
            };

            var r = p.Parse(args);
            if (inFilename == null || outFilename == null || r.Any()) {
                Console.WriteLine("Cil2JsCon");
                Console.WriteLine("Convert .NET library to JavaScript");
                Console.WriteLine();
                Console.WriteLine("Options:");
                p.WriteOptionDescriptions(Console.Out);
                return 1;
            }

            var jsResult = Transcoder.ToJs(inFilename, verbose);
            var js = jsResult.Js;
            try {
                File.WriteAllText(outFilename, js, Encoding.UTF8);
            } catch (Exception e) {
                Console.WriteLine("Error:");
                Console.WriteLine(e);
            }
            return 0;
        }

    }

}
