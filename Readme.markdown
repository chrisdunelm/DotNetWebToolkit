Cil2Js
======

The project aim is to convert libraries/executables written C#, F#, VB, *(...add your own .NET langauge here)*
to JavaScript.

Tooling support for Visual Studio may one day be provided that allows a project to be automatically converted to JavaScript
during a build, with the JavaScript output automatically being updated in a web project.

In my more optimistic moments I imagine that I'll be able to implement source-code level debugging,
with Visual Studio controlling the browsers execution of the generated JavaScript...

Current Status
--------------

Cil2Js is currently mostly non-functional. Only simple methods may be successully converted to JavaScript.
This means:

- Methods cannot call any other methods
- Methods must be static
- Only primitive types can be used (e.g. boolean, int, double, etc...)
- *...lots of things probably don't work*
- No Visual Studio tooling available
- Some of the JavaScript produced is of terrible quality

How to use
----------
Download and build the Visual Studio 2010 solution.
The library **Cil2Js** allows individual methods to be converted to JavaScript:

``` C#
using System;
using System.Reflection;
using Cil2Js;

namespace Test {
  class Program {

    static int Fn() {
      return 7;
    }

	static void Main(string[] args) {
	  MethodInfo methodInfo = typeof(Program).GetMethod("Fn", BindingFlags.NonPublic|BindingFlags.Static);
	  string javaScript = Transcoder.ToJs(methodInfo, "Fn", null);
	  Console.WriteLine(javaScript);
	}

  }
}
```

This will print:

``` JavaScript
function Fn() {
    return 7;
}
```

to the console.

----

Cil2Js uses Mono.Cecil internally for reading .NET binaries, and the transcoder will accept a
Cecil MethodDefinition:

``` C#
MethodDefinition method = ...;
string javaScript = Transcoder.ToJs(method, "Fn", null);
```

AST representation
------------------

Internally an AST (abstract syntax tree) is generated that represents the .NET CIL byte-code.
The transcoder allows this to be retreived, and the ShowVisitor helper class will print the AST:

``` C#
using System;
using System.Reflection;
using Cil2Js;

namespace Test {
  class Program {

    static int T0(int a, int b) {
      if (a == b) {
        b++;
      }
      return b;
    }

    static void Main(string[] args) {
      MethodInfo methodInfo = typeof(Program).GetMethod("T0", BindingFlags.NonPublic|BindingFlags.Static);
      MethodDefinition method = Transcoder.GetMethod(methodInfo);
      ICode ast = Transcoder.ToAst(method, null);
      var show = ShowVisitor.V(method, ast);
      Console.WriteLine(show);
	}

  }
}
```

This will print:

```
System.Int32 T0(System.Int32 a, System.Int32 b)

01d7db5d:
    if ((a == phi<49385318>(Var_028aeff3:Int32,b))) {
        Var_028aeff3:Int32 = (phi<49385318>(Var_028aeff3:Int32,b) + 1)
    }
    return phi<49385318>(Var_028aeff3:Int32,b)
}
```

AST Generation
--------------

The AST is generated iteratively.

The initial AST is a direct representation of the .NET binary;
this contains raw CIL byte-code, and the only flow-control structures are goto statements, which are
modelled as continuations in the AST.

This is iteratively transformed to the final AST in in two steps:

1. All continuations (gotos) are transformed into ifs and loops.
2. The resulting AST is simplified as much as possible.

These steps are carried out using a collection of AST visitors that are each
capable of altering the AST in a specific manner.

If you are curious about how the AST is generated, call Transcoder.ToAst() or Transcoder.ToJs()
with the 'verbose' argument set to true. This will print to the console every step on the way to
the final AST. This can produce a large amount of output for anything but the simplest methods.

Join in
=======

If this project interests you please fork, improve, and send me pull requests.

Or contact me via my profile email address.