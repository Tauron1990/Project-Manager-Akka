using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FastExpressionCompiler;
using JetBrains.Annotations;

namespace Tauron;

[PublicAPI]
public sealed class FastReflection
{
    private static readonly Lazy<FastReflection> SharedLazy = new(() => new FastReflection(), isThreadSafe: true);

    private readonly Dictionary<ConstructorInfo, Func<object?[]?, object>> _creatorCache = new();
    private readonly Dictionary<FieldInfo, Func<object?, object?>> _fieldAccessorCache = new();
    private readonly Dictionary<FieldInfo, Action<object?, object?>> _fieldSetterCache = new();
    private readonly Dictionary<MethodBase, Func<object?, object?[]?, object>> _methodCache = new();
    private readonly Dictionary<PropertyInfo, Func<object?, object[], object>> _propertyAccessorCache = new();
    private readonly Dictionary<PropertyInfo, Action<object, object?[]?, object?>> _propertySetterCache = new();

    public static FastReflection Shared => SharedLazy.Value;

    private static Expression[] CreateArgumentExpressions(ParameterInfo[] paramsInfo, Expression param)
    {
        // Pick each arg from the params array and create a typed expression of them.
        var argsExpressions = new Expression[paramsInfo.Length];

        for (var i = 0; i < paramsInfo.Length; i++)
        {
            Expression index = Expression.Constant(i);
            Type paramType = paramsInfo[i].ParameterType;
            Expression paramAccessorExp = Expression.ArrayIndex(param, index);
            Expression paramCastExp = Expression.Convert(paramAccessorExp, paramType);
            argsExpressions[i] = paramCastExp;
        }

        return argsExpressions;
    }

    public Func<object?[]?, object> GetCreator(ConstructorInfo constructor)
    {
        lock (_creatorCache)
        {
            if(_creatorCache.TryGetValue(constructor, out var func)) return func;

            // Yes, does this constructor take some parameters?
            var paramsInfo = constructor.GetParameters();

            // CreateEventActor a single param of type object[].
            ParameterExpression param = Expression.Parameter(typeof(object[]), "args");

            if(paramsInfo.Length > 0)
            {
                // Make a NewExpression that calls the constructor with the args we just created.
                UnaryExpression newExpression = Expression.Convert(Expression.New(constructor, CreateArgumentExpressions(paramsInfo, param)), typeof(object));

                // CreateEventActor a lambda with the NewExpression as body and our param object[] as arg.
                LambdaExpression lambda = Expression.Lambda(typeof(Func<object[], object>), newExpression, param);

                // Compile it
                var compiled = (Func<object?[]?, object>)lambda.CompileFast();

                _creatorCache[constructor] = compiled;

                // Success
                return compiled;
            }
            else
            {
                // Make a NewExpression that calls the constructor with the args we just created.
                UnaryExpression newExpression = Expression.Convert(Expression.New(constructor), typeof(object));

                // CreateEventActor a lambda with the NewExpression as body and our param object[] as arg.
                LambdaExpression lambda = Expression.Lambda(typeof(Func<object[], object>), newExpression, param);

                // Compile it
                var compiled = (Func<object?[]?, object>)lambda.CompileFast();

                _creatorCache[constructor] = compiled;

                // Success
                return compiled;
            }
        }
    }

    public Func<object?, object[], object?> GetPropertyAccessor(PropertyInfo info, Func<IEnumerable<Type>> arguments)
    {
        lock (_propertyAccessorCache)
        {
            if(_propertyAccessorCache.TryGetValue(info, out var invoker)) return invoker;

            var arg = arguments();

            ParameterExpression instParam = Expression.Parameter(typeof(object));
            ParameterExpression argParam = Expression.Parameter(typeof(object[]));

            Expression acess;
            UnaryExpression? convert = info.GetGetMethod()?.IsStatic == true
                ? null
                : Expression.Convert(instParam, info.DeclaringType ?? throw new InvalidOperationException($"No Declaring Type return for {info}"));

            if(!arg.Any())
                acess = Expression.Property(convert, info);
            else
                acess = Expression.Property(
                    convert,
                    info,
                    CreateArgumentExpressions(info.GetIndexParameters(), argParam));


            UnaryExpression delExp = Expression.Convert(acess, typeof(object));
            var del = Expression.Lambda<Func<object?, object[], object>>(delExp, instParam, argParam).CompileFast();

            _propertyAccessorCache[info] = del;

            return del;
        }
    }

    public Func<object?, object?> GetFieldAccessor(FieldInfo field)
    {
        lock (_fieldAccessorCache)
        {
            if(_fieldAccessorCache.TryGetValue(field, out var accessor)) return accessor;

            ParameterExpression param = Expression.Parameter(typeof(object));

            var del = Expression.Lambda<Func<object?, object?>>(
                Expression.Convert(
                    Expression.Field(
                        field.IsStatic
                            ? null
                            : Expression.Convert(param, field.DeclaringType ?? throw new InvalidOperationException($"No declaring Type returned for {field}")),
                        field),
                    typeof(object)),
                param).CompileFast();

            _fieldAccessorCache[field] = del;

            return del;
        }
    }

    public Action<object, object?[]?, object?> GetPropertySetter(PropertyInfo info)
    {
        lock (_propertySetterCache)
        {
            if(_propertySetterCache.TryGetValue(info, out var setter)) return setter;

            ParameterExpression instParam = Expression.Parameter(typeof(object));
            ParameterExpression argsParam = Expression.Parameter(typeof(object[]));
            ParameterExpression valueParm = Expression.Parameter(typeof(object));

            var indexes = info.GetIndexParameters();

            UnaryExpression convertInst = Expression.Convert(instParam, info.DeclaringType ?? throw new InvalidOperationException($"No Declared Type returned for {info}"));
            UnaryExpression convertValue = Expression.Convert(valueParm, info.PropertyType);

            Expression exp = indexes.Length == 0
                ? Expression.Assign(Expression.Property(convertInst, info), convertValue)
                : Expression.Assign(
                    Expression.Property(
                        convertInst,
                        info,
                        CreateArgumentExpressions(info.GetIndexParameters(), argsParam)),
                    convertValue);

            setter = Expression.Lambda<Action<object, object?[]?, object?>>(exp, instParam, argsParam, valueParm)
               .CompileFast();

            _propertySetterCache[info] = setter ?? throw new InvalidOperationException("Lambda Compilation Failed");

            return setter;
        }
    }

    public Action<object?, object?> GetFieldSetter(FieldInfo info)
    {
        lock (_fieldSetterCache)
        {
            if(_fieldSetterCache.TryGetValue(info, out var setter)) return setter;

            ParameterExpression instParam = Expression.Parameter(typeof(object));
            ParameterExpression valueParam = Expression.Parameter(typeof(object));

            BinaryExpression exp = Expression.Assign(
                Expression.Field(
                    Expression.Convert(instParam, info.DeclaringType ?? throw new InvalidOperationException($"no Declared Type return for {info}")),
                    info),
                Expression.Convert(valueParam, info.FieldType));

            setter = Expression.Lambda<Action<object?, object?>>(exp, instParam, valueParam).CompileFast();
            _fieldSetterCache[info] = setter;

            return setter;
        }
    }

    public Func<object?, object?[]?, object?> GetMethodInvoker(MethodInfo info, Func<IEnumerable<Type?>> arguments)
    {
        lock (_methodCache)
        {
            if(_methodCache.TryGetValue(info, out var accessor)) return accessor;

            var args = arguments().Where(t => t != null).ToArray();

            ParameterExpression instParam = Expression.Parameter(typeof(object));
            ParameterExpression argsParam = Expression.Parameter(typeof(object[]));
            UnaryExpression? convert = info.IsStatic
                ? null
                : Expression.Convert(instParam, info.DeclaringType ?? throw new InvalidOperationException($"No Declating Type for {info} Returned"));

            Expression targetExpression = args.Length == 0
                ? Expression.Call(convert, info)
                : Expression.Call(convert, info, CreateArgumentExpressions(info.GetParameters(), argsParam));

            if(info.ReturnType == typeof(void))
            {
                LabelTarget label = Expression.Label(typeof(object));
                LabelExpression labelExpression = Expression.Label(label, Expression.Constant(null, typeof(object)));

                targetExpression = Expression.Block(
                    Enumerable.Empty<ParameterExpression>(),
                    targetExpression,
                    //Expression.AndReturn(label, Expression.Constant(null), typeof(object)),
                    labelExpression);
            }
            else
            {
                targetExpression = Expression.Convert(targetExpression, typeof(object));
            }

            accessor = Expression.Lambda<Func<object?, object?[]?, object>>(targetExpression, instParam, argsParam)
               .CompileFast();
            _methodCache[info] = accessor;

            return accessor;
        }
    }

    public Func<object[], object>? GetCreator(Type target, Type[] arguments)
    {
        // Get constructor information?
        ConstructorInfo? constructor = target.GetConstructor(arguments);

        // Is there at least 1?
        return constructor == null ? null : GetCreator(constructor);
    }

    public object? FastCreateInstance(Type target, params object[] parm)
        => GetCreator(target, parm.Select(o => o.GetType()).ToArray())?.Invoke(parm);

    public object? FastCreateInstance(Type target, Type[] parmTypes, params object[] parm)
        => GetCreator(target, parmTypes)?.Invoke(parm);

    public TType FastCreateInstance<TType>(params object[] parm)
        => FastCreateInstance<TType>(parm.Select(o => o.GetType()).ToArray(), parm);

    public TType FastCreateInstance<TType>(Type[] parmTypes, params object[] parm)
    {
        object? obj = FastCreateInstance(typeof(TType), parmTypes, parm);

        if(obj is TType result)
            return result;

        throw new InvalidOperationException("No Instance was Created");
    }
}