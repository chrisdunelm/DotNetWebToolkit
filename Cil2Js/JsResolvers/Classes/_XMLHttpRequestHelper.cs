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
console.log('++encode()');
id = 0;
if (arg && typeof(arg) == 'object') {
    arg.$$ = '0';
}
todo = [arg];
todoOfs = 0;
json = [];
isObjRef = function(o) {
console.log('++isObRef()');
    if (o === null || o === undefined) {
        return false;
    }
    if (typeof(o) === 'object' && o._ && !o._.typeDataIsValueType) {
        if (!o.$$) {
            o.$$ = (++id).toString();
console.log('assign id: '+o.$$);
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
console.log('key:'+key+'; oKey:'+oKey);
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
console.log('++enc()');
    if (o === null || o === undefined) {
        return null;
    }
    var type = o._;
    if (!type) {
        if (typeof(o) === 'object' && !(o instanceof Array)) { // unboxed value-type
            return encObj(o);
            //return ret;
        } else { // string or unboxed primitive
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
        return [type.typeDataJsName, o.v];
    } else { // object, boxed value-type
console.log('obj/bvt');
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
            var i = ctx.Local(ctx.Int32, "i");
            var typeDataIsArray = new ExprJsTypeData(ctx, TypeData.IsArray).Named("typeDataIsPrimitive");
            var js = @"
objs = {};
refs = [];
dec = function(o) {
    var ret, i;
    if (o !== null && o._ !== undefined) {
        if (o._ && o._.typeDataIsArray) { // Array
            ret = new Array(o.length);
            ret._ = $$[o._];
            // TODO: Set $
            for (i = 0; i < o.length; i++) {
                ret[i] = dec(o[i]);
            }
        } else { // Object or boxed struct
            ret = { _: $$[o._] };
            // TODO: Set $ = hash id
            for (i in o) {
                if (i !== '_') {
                    if (o[i] && o[i].length === 1) {
                        // obj ref
                        refs.push(function() {
                        });
                    } else {
                        ret[i] = dec(o[i]);
                    }
                }
            }
        }
    } else { // unboxed primitive or null
        ret = o;
    }
    return ret;
};
for (i = 0; i < arg.length; i++) {
    objs[arg[i][0]] = dec(arg[i][1]);
}
return objs['0'];
";
            var stmt = new StmtJsExplicit(ctx, js, arg, objs, refs, dec, type, decTyped, ret, o, i, typeDataIsArray);
            return stmt;
        }

    }

}
