DotNet Web Toolkit
===================

DotNet Web Toolkit will be a toolkit for building web projects entirely in .NET languages , for example C#, F#, Visual Basic, ...

The compiled code is converted to JavaScript that will run on all modern web browsers.

The toolkit will provide all the APIs available within the web browser, allowing code like this:

```C#
public static void Canvas2DDemo() {
    var canvas = (HtmlCanvasElement)Document.GetElementById("canvasId");
    var ctx = (CanvasRenderingContext2D)canvas.GetContext(CanvasContext.TwoD);
    string fill1 = "#ff0000";
    string fill2 = "#0000ff";
    bool f1 = true;
    Window.SetInterval(() => {
        ctx.FillStyle = f1 ? fill1 : fill2;
        f1 = !f1;
        ctx.FillRect(50, 50, 100, 50);
    }, 5000);
}
```

to be converted to javascript.

The toolkit will provide the facility to integrate with web projects allowing specified source code to target the browser and be automatically converted to JavaScript during the build process.

The tookit will provide source-level debugging of in-broswer javascript; for example allowing the generated JavaScript running in the browser to be debugged from within Visual Studio with breakpoints and execution control occuring in C#/F#/VB/... source code.

Current status
--------------

Currently Dot Net Web Toolkit provides:

* ***Cil2Js***: A [CIL][1] *(.NET bytecode)* to JavaScript converter
* ***DotNetWebToolkit***: Provides APIs available with the web browser

*Cil2JS*
--------

Cil2Js is functional and can convert many CIL methods to JavaScript. For example, the C# method:

```C#
public static int Factorial(int i) {
    return (i == 0) ? 1 : i * Factorial(i - 1);
}
```

is compiled, then transcoded to JavaScript:

```JavaScript
var main = function(a) {
    var b = 0, c = 0;
    if ((!a)) {
        b = 1;
    } else {
        c = main((a - 1));
        b = (a * c);
    }
    return b;
}
```

*(note that the method passed to be transcoded is currently always renamed 'main')*

Many .NET features and types can be used, but the following are currently not implemented:

* Some nested try/catch/finally structures; specifically if a CIL 'leave' instruction breaks out of more than one try statement.
* Many low-level .NET base class type methods are partially implemeneted by the .NET runtime itself, rather than being implemented in CIL *(e.g. int, string)*. These all have be re-implemented by the toolkit before they can be transcoded to JavaScript. This has been done of some methods, but many remain to be written.
* Some .NET base class type methods use unsafe code, which will probably never be supported by Cil2Js *(e.g. Many methods in StringBuilder)*. Again, these require re-implementing within the toolkit before they can be transcoded. 

The following also requires improving:

* The JavaScript produced is not optimal. In fact, some of it is terrible.
* All JavaScript functions are currently in the global namespace.
* Some CIL cannot be transcode.

*DotNetWebToolkit*
------------------

This library is referenced in projects to be transcoded to JavaScript and provides APIs found in web browsers.

For example it provides a ***Document*** class with the *GetElementById()* method, allowing:

```C#
using DotNetWebToolkit.Web;

...

HtmlElement element = Document.GetElementById("canvasId");
```

This library is currently extremely incomplete.


How to use
----------

```C#
using DotNetWebToolkit.Cil2Js;

...

public static int Factorial(int i) {
    return (i == 0) ? 1 : i * Factorial(i - 1);
}

public static int Fibonacci(int i) {
    return (i <= 1) ? 1 : Fibonacci(i - 1) + Fibonacci(i - 2);
}

public static int FactorialOrFibonacci(int i, bool factorial) {
    if (factorial) {
        return Factorial(i);
    } else {
        return Fibonacci(i);
    }
}

static void Main(string[] args) {
    MethodInfo mi = typeof(Program).GetMethod("FactorialOrFibonacci");
    string js = Transcoder.ToJs(mi, true);
    Console.WriteLine(js);
}
```

The static method ***Transcoder***.*ToJs()* converts the passed method to JavaScript, including all called methods:

```JavaScript
var main = function(a, c) {
    var d = 0, b = 0, f = 0;
    if ((!c)) {
        d = e(a);
        b = d;
    } else {
        f = g(a);
        b = f;
    }
    return b;
}

var g = function(a) {
    var b = 0, c = 0;
    if ((!a)) {
        b = 1;
    } else {
        c = g((a - 1));
        b = (a * c);
    }
    return b;
}

var e = function(a) {
    var b = 0, c = 0, d = 0;
    if ((a <= 1)) {
        b = 1;
    } else {
        c = e((a - 1));
        d = e((a - 2));
        b = (c + d);
    }
    return b;
}
```

How it works
============

The CIL is loaded using [Mono.Cecil][2] and converted to an [AST][3] representation. This AST is then almost directly written out as JavaScript.

Creating the AST is performed in multiple steps. The first step breaks the CIL into its [basic blocks][4]. This AST contains raw CIL and flow control uses unstructured goto statements. This is transformed into as AST that contains only structured flow control using multiple AST visitors, each of which transform the AST in a specific way. This process can be seen in the *Cil2Js* ***Transcoder***.*ToAst()* method.

Once the AST has been generated, it must be further transformed as required for converting to JavaScript.

* Methods and types are mapped as required to alternative methods/types to allow unconvertable methods in the .NET base clases to use alternative implementations provided by the toolkit.
* Value-type use is altered to allow JavaScript to conform to the expected .NET value-type semantics.
* Cast and type checks *(cast and isinst CIL instructions)* are re-written as functions, with the required runtime type information also emitted in the JavaScript.

To see the steps required to convert a .NET method to JavaScript, the ***Transcoder***.*ToJs()* method can be called with the *verbose* argument set to *true*. This will print out all the AST transformations - *this can produce a lot of output*.

Join in the fun
===============

If this project interests you please fork, improve, and send a pull request.

Or contact me via my profile email address.


[1]: http://en.wikipedia.org/wiki/Common_Intermediate_Language
[2]: https://github.com/jbevain/cecil
[3]: http://en.wikipedia.org/wiki/Abstract_syntax_tree
[4]: http://en.wikipedia.org/wiki/Basic_block
