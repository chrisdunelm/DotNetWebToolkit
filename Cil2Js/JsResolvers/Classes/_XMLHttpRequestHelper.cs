using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    class _XMLHttpRequestHelper {

        [Js]
        public static Stmt Encode(Ctx ctx) {
            var arg = ctx.MethodParameter(0, "arg");
            var id = ctx.Local(ctx.Int32, "id");
            var todo = ctx.Local(ctx.Object, "todo");
            var todoOfs = ctx.Local(ctx.Int32, "todoOfs");
            var enc = ctx.Local(ctx.Object, "enc");
            var obj = ctx.Local(ctx.Object, "obj");
            var json = ctx.Local(ctx.Object, "json");
            var jsonPart = ctx.Local(ctx.Object, "jsonPart");
            var o = ctx.Local(ctx.Object, "o");
            var type = ctx.Local(ctx.Type, "type");
            var oKey = ctx.Local(ctx.String, "oKey");
            var isRoot = ctx.Local(ctx.Boolean, "isRoot");
            var i = ctx.Local(ctx.Int32, "i");
            var isArray = new ExprJsTypeData(ctx, TypeData.IsArray).Named("isArray");
            var jsName = new ExprJsTypeData(ctx, TypeData.JsName).Named("jsName");
            var ret = ctx.Local(ctx.Object, "ret");
            // Value-types will be boxed on entry
            var js = @"
try {
id = 0;
if (arg && arg._) {
    arg.$$ = '0';
}
todo = [arg];
todoOfs = 0;
enc = function(o, isRoot) {
    if (o === null || o === undefined) {
        return null;
    }
    var type = o._;
    if (!type) { // Unboxed value-type/primitive
        if (typeof(o) !== 'object' || o instanceof Array) { // primitive number, boolean, string, 64-bit number (array)
            if (typeof(o) === 'number') { // Number
                if (isNaN(o)) {
                    return [0];
                }
                if (o === Number.NEGATIVE_INFINITY) {
                    return [-1];
                }
                if (o === Number.POSITIVE_INFINITY) {
                    return [1];
                }
            }
            return o;
        } else {
            // Non-primitive value-type
            var ret = {};
            for (var oKey in o) {
                ret[oKey] = enc(o[oKey]);
            }
            return ret;
        }
    }
    if (isRoot) {
        // Direct object encoding required
        if (type && type.isArray) {
            var ret = { '_': type.jsName, 'v': [] };
            for (var i=0; i<o.length; i++) {
                ret.v.push(enc(o[i]));
            }
            return ret;
        } else {
            var ret = { '_': type.jsName };
            for (var oKey in o) {
                if (oKey.charAt(0) !== '_' && oKey.charAt(0) !== '$') {
                    ret[oKey] = enc(o[oKey]);
                }
            }
            return ret;
        }
    } else {
        if (!o.$$) {
            o.$$ = (++id).toString();
            todo.push(o);
        }
        return [o.$$];
    }
};
json = [];
while (todo.length > todoOfs) {
    obj = todo[todoOfs++];
    jsonPart = enc(obj, true);
    json.push([(obj && obj.$$) || '0', jsonPart]);
}
for (i = 0; i<todo.length; i++) {
    if (todo[i] && todo[i].$$) {
        delete todo[i].$$;
    }
}
return json;
} catch (eeee) {
console.log('Ex: ' + eeee);
throw eeee;
}
";
            var stmt = new StmtJsExplicit(ctx, js, arg, id, todo, todoOfs, enc, obj, json, jsonPart, o, type, oKey, isRoot, i, isArray, ret, jsName);
            return stmt;
        }

        [Js]
        public static Stmt Decode(Ctx ctx) {
            var arg = ctx.MethodParameter(0, "arg");
            var objs = ctx.Local(ctx.Object, "objs");
            var refs = ctx.Local(ctx.Object, "refs");
            var needDefer = ctx.Local(ctx.Object, "needDefer");
            var defer = ctx.Local(ctx.Object, "defer");
            var dec = ctx.Local(ctx.Object, "dec");
            var type = ctx.Local(ctx.Type, "type");
            var ret = ctx.Local(ctx.Object, "ret");
            var o = ctx.Local(ctx.Object, "o");
            var oArray = ctx.Local(ctx.Object, "oArray");
            var i = ctx.Local(ctx.Int32, "i");
            var isObject = ctx.Local(ctx.Boolean, "isObject");
            var js = @"
objs = {};
refs = [];
needDefer = function(o) {
    return !!(o && o instanceof Array &&  o.length === 1 && typeof(o[0]) === 'string');
};
defer = function(ret, i, o) {
    if (needDefer(o[i])) {
        refs.push(function() {
            ret[i] = objs[o[i][0]];
        });
    } else {
        ret[i] = dec(o[i]);
    }
};
dec = function(o) {
    var ret, i;
    if (o == null) {
        return null;
    }
    var isObject = false;
    if (o._ !== undefined) {
        if (o.__ === 2) { // Dictionary
            ret = $d[o.d][0]({ _: $$[o._] });
            for (var i = 0; i < o.a.length; i++) {
                var kVal = o.a[i];
                var vVal = o.b[i];
                var kRef = needDefer(kVal);
                var vRef = needDefer(vVal);
                (function(kRef, kVal, vRef, vVal, o, ret) {
                    refs.push(function() {
                        if (kRef) kVal = objs[kVal[0]];
                        if (vRef) vVal = objs[vVal[0]];
                        $d[o.d][1](ret, kVal, vVal);
                    });
                })(kRef, kVal, vRef, vVal, o, ret);
            }
        } else if (o['']) { // Array
            var oArray = o[''];
            var ret = new Array(oArray.length);
            ret._ = $$[o._];
            // TODO: Set $
            for (var i = 0; i < oArray.length; i++) {
                defer(ret, i, oArray)
            }
        } else { // Object or boxed struct
            isObject = true;
        }
    } else if (typeof(o) === 'object' && !(o instanceof Array)) { // unboxed value-type
        isObject = true;
    } else if (o instanceof Array && o.length === 1) { // Special Single/Double
        switch (o[0]) {
        case 0: ret = NaN; break;
        case 1: ret = Number.POSITIVE_INFINITY; break;
        case -1: ret = Number.NEGATIVE_INFINITY; break;
        default: throw 'Unrecognised special: ' + o[0];
        }
    } else { // unboxed primitive or null
        ret = o;
    }
    if (isObject) {
        var ret = o._ ? { '_': $$[o._] } : { };
        // TODO: Set $ = hash id
        for (var i in o) {
            if (i !== '_') {
                defer(ret, i, o);
            }
        }
    }
    return ret;
};
for (i = 0; i < arg.length; i++) {
    objs[arg[i][0]] = dec(arg[i][1]);
}
for (i = 0; i < refs.length; i++) {
    refs[i]();
}
ret = objs['0'];
return ret;
";
            var stmt = new StmtJsExplicit(ctx, js, arg, objs, refs, needDefer, defer, dec, type, ret, o, oArray, i, isObject);
            return stmt;
        }

    }

}
