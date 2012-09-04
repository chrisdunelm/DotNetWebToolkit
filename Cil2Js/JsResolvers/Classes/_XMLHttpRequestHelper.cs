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
            var todo = ctx.Local(ctx.Object, "todo");
            var todoOfs = ctx.Local(ctx.Int32, "todoOfs");
            var id = ctx.Local(ctx.Int32, "id");
            var json = ctx.Local(ctx.Object, "json");
            var obj = ctx.Local(ctx.Object, "obj");
            var type = ctx.Local(ctx.Type, "type");
            var jsonObj = ctx.Local(ctx.Object, "jsonObj");
            var field = ctx.Local(ctx.Object, "field");
            var fieldKey = ctx.Local(ctx.String, "fieldKey");
            var typeDataIsArray = new ExprJsTypeData(ctx, TypeData.IsArray).Named("typeDataIsArray");
            var typeDataIsPrimitive = new ExprJsTypeData(ctx, TypeData.IsPrimitive).Named("typeDataIsPrimitive");
            var typeDataIsValueType = new ExprJsTypeData(ctx, TypeData.IsValueType).Named("typeDataIsValueType");
            var typeDataJsName = new ExprJsTypeData(ctx, TypeData.JsName).Named("typeDataJsName");
            var stringType = new ExprJsTypeVarName(ctx, ctx.String).Named("stringType");
            var isObjRef = ctx.Local(ctx.Object, "isObjRef");
            var enc = ctx.Local(ctx.Object, "enc");
            var encObj = ctx.Local(ctx.Object, "encObj");
            var o = ctx.Local(ctx.Object, "o");
            var ret = ctx.Local(ctx.Object, "ret");
            var i = ctx.Local(ctx.Int32, "i");
            var key = ctx.Local(ctx.String, "key");
            var oKey = ctx.Local(ctx.Object, "oKey");
            var c = ctx.Local(ctx.Char, "c");
            var js = @"
try {
id = 0;
if (arg && typeof(arg) == 'object') {
    arg.$$ = '0';
}
todo = [arg];
todoOfs = 0;
json = [];
isObjRef = function(o) {
    if (o === null || o === undefined) {
        return false;
    }
    if (typeof(o) === 'object' && o._ && !o._.typeDataIsValueType) {
        if (!o.$$) {
            o.$$ = (++id).toString();
            todo.push(o);
        }
        return true;
    }
    return false;
};
encObj = function(o) {
    var ret = [];
    for (var key in o) {
        var c = key.charAt(0);
        if (c !== '_' && c !== '$' && o.hasOwnProperty(key)) {
            var oKey = o[key];
            if (isObjRef(oKey)) {
                ret.push(key, [oKey.$$]);
            } else {
                ret.push(key, enc(oKey));
            }
        }
    }
    return ret;
};
enc = function(o) {
    if (o === null || o === undefined) {
        return null;
    }
    var type = o._;
    if (!type) {
        if (typeof(o) === 'object' && !(o instanceof Array)) { // unboxed value-type
            return encObj(o);
        } else { // string or unboxed primitive
            if (typeof(o) === 'number') {
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
        }
    } else if (type.typeDataIsArray) { // array
        var ret = new Array(o.length);
        for (var i = 0; i < o.length; i++) {
            var oKey = o[i];
            if (isObjRef(oKey)) {
                ret[i] = [oKey.$$];
            } else {
                ret[i] = enc(o[i]);
            }
        }
        return [type.typeDataJsName, ret];
    } else if (type.typeDataIsPrimitive) { // boxed primitive, supports 64-bit numbers without any special code
        return [type.typeDataJsName, enc(o.v)];
    } else { // object, boxed value-type
        if (type.typeDataIsValueType) {
            o = o.v;
        }
        return [type.typeDataJsName, encObj(o)];
    }
};
while (todo.length > todoOfs) {
    obj = todo[todoOfs++];
    json.push([(obj && typeof(obj) == 'object' && obj.$$) ? obj.$$ : '0', enc(obj)]);
}
for (todoOfs = 0; todoOfs < todo.length; todoOfs++) {
    obj = todo[todoOfs];
    if (obj && typeof(obj) == 'object') {
        delete todo[todoOfs].$$;
    }
}
return json;
} catch (xx) {
console.log('EX:'+xx);
throw xx;
}
";
            var stmt = new StmtJsExplicit(ctx, js, arg, todo, todoOfs, id, json, obj, type, jsonObj, field, fieldKey, typeDataIsArray, typeDataIsPrimitive, typeDataIsValueType, typeDataJsName, stringType, isObjRef, enc, encObj, o, ret, i, key, oKey, c);
            return stmt;
        }

        [Js]
        public static Stmt Decode(Ctx ctx) {
            var arg = ctx.MethodParameter(0, "arg");
            var objs = ctx.Local(ctx.Object, "objs");
            var refs = ctx.Local(ctx.Object, "refs");
            var dec = ctx.Local(ctx.Object, "dec");
            var type = ctx.Local(ctx.Type, "type");
            var decTyped = ctx.Local(ctx.Object, "decTyped");
            var ret = ctx.Local(ctx.Object, "ret");
            var o = ctx.Local(ctx.Object, "o");
            var oArray = ctx.Local(ctx.Object, "oArray");
            var i = ctx.Local(ctx.Int32, "i");
            var isObject = ctx.Local(ctx.Boolean, "isObject");
            var typeDataIsArray = new ExprJsTypeData(ctx, TypeData.IsArray).Named("typeDataIsPrimitive");
            var js = @"
objs = {};
refs = [];
dec = function(o) {
    var ret, i;
    if (o == null) {
        return null;
    }
    var isObject = false;
    if (o._ !== undefined) {
        if (o['']) { // Array
            var oArray = o[''];
            var ret = new Array(oArray.length);
            ret._ = $$[o._];
            // TODO: Set $
            for (var i = 0; i < oArray.length; i++) {
                if (oArray[i] && oArray[i].length === 1 && typeof(oArray[i][0]) === 'string') {
                    // obj ref
                    (function(ret, i, o) {
                        refs.push(function() {
                            ret[i] = objs[oArray[i]];
                        });
                    })(ret, i, o);
                } else {
                    ret[i] = dec(oArray[i]);
                }
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
        var ret = { '_': o._ ? $$[o._] : null };
        // TODO: Set $ = hash id
        for (var i in o) {
            if (i !== '_') {
                if (o[i] && o[i].length === 1 && typeof(o[i][0]) === 'string') {
                    // obj ref
                    (function(ret, i, o) {
                        refs.push(function() {
                            ret[i] = objs[o[i]];
                        });
                    })(ret, i, o)
                } else {
                    ret[i] = dec(o[i]);
                }
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
            var stmt = new StmtJsExplicit(ctx, js, arg, objs, refs, dec, type, decTyped, ret, o, oArray, i, isObject, typeDataIsArray);
            return stmt;
        }

    }

}
