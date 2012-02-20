using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class NameGenerator {

        private readonly static HashSet<string> reservedWords = new HashSet<string>(new[]{
            "break",
            "continue",
            "do",
            "for",
            "import",
            "new",
            "this",
            "void",
            "case",
            "default",
            "else",
            "function",
            "in",	
            "return",	
            "typeof",
            "while",
            "comment"	,
            "delete",
            "export",
            "if",
            "label",
            "switch",
            "var",	
            "with",
            "abstract",
            "implements",
            "protected",
            "boolean",
            "instanceOf",
            "public",
            "byte",
            "int",
            "short",
            "char",
            "interface",
            "static",
            "double",
            "long",
            "synchronized",
            "false",
            "native",
            "throws",
            "final",
            "null",
            "transient",
            "float",
            "package",
            "true",
            "goto",
            "private",
            "catch",
            "enum",
            "throw",
            "class",
            "extends",
            "try",
            "const",
            "finally",
            "debugger",
            "super",
            "alert",
            "eval",
            "Link",
            "outerHeight",
            "scrollTo",
            "Anchor",
            "FileUpload",
            "location",
            "outerWidth",
            "Select",
            "Area",
            "find",
            "Location",
            "Packages",
            "self",
            "arguments",
            "focus",
            "locationbar",
            "pageXoffset",
            "setInterval",
            "Array",
            "Form",
            "Math",
            "pageYoffset",
            "setTimeout",
            "assign",
            "Frame",
            "menubar",
            "parent",
            "status",
            "blur",
            "frames",
            "MimeType",
            "parseFloat",
            "statusbar",
            "Boolean",
            "Function",
            "moveBy",
            "parseInt",
            "stop",
            "Button",
            "getClass",
            "moveTo",
            "Password",
            "String",
            "callee",
            "Hidden",
            "name",
            "personalbar",
            "Submit",
            "caller",
            "history",
            "NaN",
            "Plugin",
            "sun",
            "captureEvents",
            "History",
            "navigate",
            "print",
            "taint",
            "Checkbox",
            "home",
            "navigator",
            "prompt",
            "Text",
            "clearInterval",
            "Image",
            "Navigator",
            "prototype",
            "Textarea",
            "clearTimeout",
            "Infinity",
            "netscape",
            "Radio",
            "toolbar",
            "close",
            "innerHeight",
            "Number",
            "ref",
            "top",
            "closed",
            "innerWidth",
            "Object",
            "RegExp",
            "toString",
            "confirm",
            "isFinite",
            "onBlur",
            "releaseEvents",
            "unescape",
            "constructor",
            "isNan",
            "onError",
            "Reset",
            "untaint",
            "Date",
            "java",
            "onFocus",
            "resizeBy",
            "unwatch",
            "defaultStatus",
            "JavaArray",
            "onLoad",
            "resizeTo",
            "valueOf",
            "document",
            "JavaClass",
            "onUnload",
            "routeEvent",
            "watch",
            "Document",
            "JavaObject",
            "open",
            "scroll",
            "window",
            "Element",
            "JavaPackage",
            "opener",
            "scrollbars",
            "Window",
            "escape",
            "length",
            "Option",
            "scrollBy"
        });

        private readonly static char[] c0 = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToArray();
        private readonly static char[] cN = c0.Concat("0123456789".ToArray()).ToArray();

        public NameGenerator(IEnumerable<string> forbidden = null) {
            this.gen = this.GenFn(forbidden == null ? null : new HashSet<string>(forbidden)).GetEnumerator();
        }

        private IEnumerator<string> gen;

        private IEnumerable<string> GenFn(HashSet<string> forbidden) {
            int[] indexes = { 0 };
            for (; ; ) {
                var name = new string(indexes.Select((index, position) => (position == 0 ? c0 : cN)[index]).ToArray());
                if (!reservedWords.Contains(name) && (forbidden == null || !forbidden.Contains(name))) {
                    yield return name;
                }
                bool ok = false;
                for (int i = indexes.Length - 1; i >= 0; i--) {
                    if (++indexes[i] >= (i == 0 ? c0 : cN).Length) {
                        indexes[i] = 0;
                    } else {
                        ok = true;
                        break;
                    }
                }
                if (!ok) {
                    // Need to start a longer name
                    indexes = new int[indexes.Length + 1]; // All default to 0 which is correct
                }
            }
        }

        public string GetNewName() {
            this.gen.MoveNext();
            return this.gen.Current;
        }


    }
}
