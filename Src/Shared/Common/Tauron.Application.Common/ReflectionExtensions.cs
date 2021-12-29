﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FastExpressionCompiler;
using JetBrains.Annotations;
using Stl;

namespace Tauron;

[PublicAPI]
public static class Reflex
{
    public static MethodInfo MethodInfo<T>(Expression<Action<T>> expression)
    {
        if (expression.Body is MethodCallExpression member)
            return member.Method;

        throw new ArgumentException("Expression is not a method", nameof(expression));
    }

    public static MethodInfo MethodInfo(Expression<Action> expression)
    {
        if (expression.Body is MethodCallExpression member)
            return member.Method;

        throw new ArgumentException("Expression is not a method", nameof(expression));
    }

    public static string PropertyName<T>(Expression<Func<T>> propertyExpression)
    {
        var memberExpression = (MemberExpression)propertyExpression.Body;

        return memberExpression.Member.Name;
    }

    public static string PropertyName<TIn, T>(Expression<Func<TIn, T>> propertyExpression)
    {
        var memberExpression = (MemberExpression)propertyExpression.Body;

        return memberExpression.Member.Name;
    }
}

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
            var paramType = paramsInfo[i].ParameterType;
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
            if (_creatorCache.TryGetValue(constructor, out var func)) return func;

            // Yes, does this constructor take some parameters?
            var paramsInfo = constructor.GetParameters();

            // CreateEventActor a single param of type object[].
            var param = Expression.Parameter(typeof(object[]), "args");

            if (paramsInfo.Length > 0)
            {
                // Make a NewExpression that calls the constructor with the args we just created.
                var newExpression = Expression.New(constructor, CreateArgumentExpressions(paramsInfo, param));

                // CreateEventActor a lambda with the NewExpression as body and our param object[] as arg.
                var lambda = Expression.Lambda(typeof(Func<object[], object>), newExpression, param);

                // Compile it
                var compiled = (Func<object?[]?, object>)lambda.CompileFast();

                _creatorCache[constructor] = compiled;

                // Success
                return compiled;
            }
            else
            {
                // Make a NewExpression that calls the constructor with the args we just created.
                var newExpression = Expression.New(constructor);

                // CreateEventActor a lambda with the NewExpression as body and our param object[] as arg.
                var lambda = Expression.Lambda(typeof(Func<object[], object>), newExpression, param);

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
            if (_propertyAccessorCache.TryGetValue(info, out var invoker)) return invoker;

            var arg = arguments();

            var instParam = Expression.Parameter(typeof(object));
            var argParam = Expression.Parameter(typeof(object[]));

            Expression acess;
            var convert = info.GetGetMethod()?.IsStatic == true
                ? null
                : Expression.Convert(instParam, info.DeclaringType ?? throw new InvalidOperationException($"No Declaring Type return for {info}"));

            if (!arg.Any())
                acess = Expression.Property(convert, info);
            else
                acess = Expression.Property(
                    convert,
                    info,
                    CreateArgumentExpressions(info.GetIndexParameters(), argParam));


            var delExp = Expression.Convert(acess, typeof(object));
            var del = Expression.Lambda<Func<object?, object[], object>>(delExp, instParam, argParam).CompileFast();

            _propertyAccessorCache[info] = del;

            return del;
        }
    }

    public Func<object?, object?> GetFieldAccessor(FieldInfo field)
    {
        lock (_fieldAccessorCache)
        {
            if (_fieldAccessorCache.TryGetValue(field, out var accessor)) return accessor;

            var param = Expression.Parameter(typeof(object));

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
            if (_propertySetterCache.TryGetValue(info, out var setter)) return setter;

            var instParam = Expression.Parameter(typeof(object));
            var argsParam = Expression.Parameter(typeof(object[]));
            var valueParm = Expression.Parameter(typeof(object));

            var indexes = info.GetIndexParameters();

            var convertInst = Expression.Convert(instParam, info.DeclaringType ?? throw new InvalidOperationException($"No Declared Type returned for {info}"));
            var convertValue = Expression.Convert(valueParm, info.PropertyType);

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
            if (_fieldSetterCache.TryGetValue(info, out var setter)) return setter;

            var instParam = Expression.Parameter(typeof(object));
            var valueParam = Expression.Parameter(typeof(object));

            var exp = Expression.Assign(
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
            if (_methodCache.TryGetValue(info, out var accessor)) return accessor;

            var args = arguments().Where(t => t != null).ToArray();

            var instParam = Expression.Parameter(typeof(object));
            var argsParam = Expression.Parameter(typeof(object[]));
            var convert = info.IsStatic
                ? null
                : Expression.Convert(instParam, info.DeclaringType ?? throw new InvalidOperationException($"No Declating Type for {info} Returned"));

            Expression targetExpression = args.Length == 0
                ? Expression.Call(convert, info)
                : Expression.Call(convert, info, CreateArgumentExpressions(info.GetParameters(), argsParam));

            if (info.ReturnType == typeof(void))
            {
                var label = Expression.Label(typeof(object));
                var labelExpression = Expression.Label(label, Expression.Constant(null, typeof(object)));

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
        var constructor = target.GetConstructor(arguments);

        // Is there at least 1?
        return constructor == null ? null : GetCreator(constructor);
    }

    public object? FastCreateInstance(Type target, params object[] parm)
        => GetCreator(target, parm.Select(o => o.GetType()).ToArray())?.Invoke(parm);

    public object? FastCreateInstance(Type target, Type[] parmTypes,  params object[] parm)
        => GetCreator(target, parmTypes)?.Invoke(parm);

    public TType FastCreateInstance<TType>(params object[] parm)
        => FastCreateInstance<TType>(parm.Select(o => o.GetType()).ToArray(), parm);

    public TType FastCreateInstance<TType>(Type[] parmTypes, params object[] parm)
    {
        var obj = FastCreateInstance(typeof(TType), parmTypes, parm);

        if (obj is TType result)
            return result;

        throw new InvalidOperationException("No Instance was Created");
    }
}

[PublicAPI]
public static class ReflectionExtensions
{
    public const BindingFlags DefaultBindingFlags =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    public static T ParseEnum<T>(this string value, bool ignoreCase)
        where T : struct
        => Enum.TryParse(value, ignoreCase, out T evalue) ? evalue : default;

    public static IEnumerable<(MemberInfo Member, TAttribute Attribute)> FindMemberAttributes<TAttribute>(
        this Type type,
        bool nonPublic,
        BindingFlags bindingflags) where TAttribute : Attribute
    {
        if (type == null) throw new ArgumentNullException(nameof(type));

        bindingflags |= BindingFlags.Public;
        if (nonPublic) bindingflags |= BindingFlags.NonPublic;

        if (!Enum.IsDefined(typeof(BindingFlags), BindingFlags.FlattenHierarchy))
            return (
                from mem in type.GetMembers(bindingflags)
                let attr = CustomAttributeExtensions.GetCustomAttribute<TAttribute>(mem)
                where attr != null
                select (mem, attr)
            );

        return
            from mem in type.GetHieratichialMembers(bindingflags)
            let attr = mem.GetCustomAttribute<TAttribute>()
            where attr.HasValue
            select (mem, attr.Value);
    }

    public static Func<object?, object?[]?, object?> GetMethodInvoker(
        this MethodInfo info,
        Func<IEnumerable<Type?>> arguments) => FastReflection.Shared.GetMethodInvoker(info, arguments);

    public static IEnumerable<MemberInfo> GetHieratichialMembers(this Type? type, BindingFlags flags)
    {
        var targetType = type;
        while (targetType != null)
        {
            foreach (var mem in targetType.GetMembers(flags)) yield return mem;

            targetType = targetType.BaseType;
        }
    }

    public static IEnumerable<(MemberInfo Member, TAttribute Attribute)> FindMemberAttributes<TAttribute>(
        this Type type,
        bool nonPublic) where TAttribute : Attribute
        => FindMemberAttributes<TAttribute>(type, nonPublic, BindingFlags.Instance | BindingFlags.FlattenHierarchy);

    public static T[] GetAllCustomAttributes<T>(this ICustomAttributeProvider member) where T : class
        => (T[])member.GetCustomAttributes(typeof(T), inherit: true);


    public static object[] GetAllCustomAttributes(this ICustomAttributeProvider member, Type type)
        => member.GetCustomAttributes(type, inherit: true);

    public static Option<TAttribute> GetCustomAttribute<TAttribute>(this ICustomAttributeProvider provider)
        where TAttribute : Attribute
        => GetCustomAttribute<TAttribute>(provider, inherit: true);

    public static Option<TAttribute> GetCustomAttribute<TAttribute>(this ICustomAttributeProvider provider, bool inherit)
        where TAttribute : Attribute
    {
        var temp = provider.GetCustomAttributes(typeof(TAttribute), inherit).FirstOrDefault();

        return (temp as TAttribute).OptionNotNull();
    }

    public static IEnumerable<object> GetCustomAttributes(this ICustomAttributeProvider provider, params Type[] attributeTypes)
    {
        if (provider == null) throw new ArgumentNullException(nameof(provider));

        return from attributeType in attributeTypes
               from attribute in provider.GetCustomAttributes(attributeType, inherit: false)
               select attribute;
    }

    public static Option<TType> GetInvokeMember<TType>(this MemberInfo info, object instance, params object[]? parameter)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));

        parameter ??= Array.Empty<object>();

        return info switch
        {
            PropertyInfo property => FastReflection.Shared
               .GetPropertyAccessor(property, () => property.GetIndexParameters().Select(pi => pi.ParameterType)).Invoke(instance, parameter) is TType pType
                ? pType.AsOption()
                : default,
            FieldInfo field => FastReflection.Shared.GetFieldAccessor(field)(instance) is TType type
                ? type.AsOption()
                : default,
            MethodInfo methodInfo => FastReflection.Shared.GetMethodInvoker(
                methodInfo,
                methodInfo.GetParameterTypes)(instance, parameter) is TType mType
                ? mType.AsOption()
                : default,
            ConstructorInfo constructorInfo =>
                FastReflection.Shared.GetCreator(constructorInfo)(parameter) is TType cType ? cType.AsOption() : default,
            _ => default!
        };
    }

    public static RuntimeMethodHandle GetMethodHandle(this MethodBase method)
    {
        if (method == null) throw new ArgumentNullException(nameof(method));

        var mi = method as MethodInfo;

        if (mi != null && mi.IsGenericMethod) return mi.GetGenericMethodDefinition().MethodHandle;

        return method.MethodHandle;
    }

    public static IEnumerable<Type> GetParameterTypes(this MethodBase method)
    {
        if (method is null) throw new ArgumentNullException(nameof(method));

        return method.GetParameters().Select(p => p.ParameterType);
    }

    public static Option<PropertyInfo> GetPropertyFromMethod(this MethodInfo method, Type implementingType)
    {
        if (method is null) throw new ArgumentNullException(nameof(method));
        if (implementingType is null) throw new ArgumentNullException(nameof(implementingType));

        if (!method.IsSpecialName || method.Name.Length < 4) return default;

        var isGetMethod = method.Name.Substring(0, 4) == "get_";
        var returnType = isGetMethod ? method.ReturnType : method.GetParameterTypes().Last();
        var indexerTypes = isGetMethod
            ? method.GetParameterTypes()
            : method.GetParameterTypes().SkipLast(1);

        return implementingType.GetProperty(method.Name[4..], DefaultBindingFlags, null, returnType, indexerTypes.ToArray(), null)
           .OptionNotNull();
    }

    public static Option<PropertyInfo> GetPropertyFromMethod(this MethodBase method)
    {
        if (method == null) throw new ArgumentNullException(nameof(method));

        return !method.IsSpecialName
            ? default
            : (method.DeclaringType?.GetProperty(method.Name[4..], DefaultBindingFlags)).OptionNotNull();
    }

    public static Type GetSetInvokeType(this MemberInfo info)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));

        return info switch
        {
            FieldInfo field => field.FieldType,
            MethodBase method => method.GetParameterTypes().Single(),
            PropertyInfo property => property.PropertyType,
            _ => throw new ArgumentOutOfRangeException(nameof(info))
        };
    }

    public static bool HasAttribute<T>(this ICustomAttributeProvider member) where T : Attribute
    {
        if (member == null) throw new ArgumentNullException(nameof(member));

        return member.IsDefined(typeof(T), inherit: true);
    }

    public static bool HasAttribute(this ICustomAttributeProvider member, Type type)
    {
        if (member == null) throw new ArgumentNullException(nameof(member));
        if (type == null) throw new ArgumentNullException(nameof(type));

        return member.IsDefined(type, inherit: true);
    }

    public static bool HasMatchingAttribute<T>(this ICustomAttributeProvider member, T attributeToMatch)
        where T : Attribute
    {
        if (member == null) throw new ArgumentNullException(nameof(member));
        if (attributeToMatch == null) throw new ArgumentNullException(nameof(attributeToMatch));

        var attributes = member.GetAllCustomAttributes<T>();

        return attributes.Length != 0 && attributes.Any(attribute => attribute.Match(attributeToMatch));
    }

    public static Option<TType> InvokeFast<TType>(this MethodBase method, object? instance, params object?[] args)
    {
        if (method == null) throw new ArgumentNullException(nameof(method));

        return method switch
        {
            MethodInfo methodInfo => FastReflection.Shared.GetMethodInvoker(
                methodInfo,
                methodInfo.GetParameterTypes)(instance, args) is TType mR
                ? mR.AsOption()
                : default,
            ConstructorInfo constructorInfo => FastReflection.Shared.GetCreator(constructorInfo)(args) is TType cr
                ? cr.AsOption()
                : default,
            _ => throw new ArgumentException(@"Method Not Supported", nameof(method))
        };
    }

    public static void InvokeFast(this MethodInfo method, object? instance, params object?[] args)
        => FastReflection.Shared.GetMethodInvoker(method, method.GetParameterTypes)(instance, args);

    public static TEnum ParseEnum<TEnum>(this string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        return (TEnum)Enum.Parse(typeof(TEnum), value);
    }

    public static TEnum TryParseEnum<TEnum>(this string value, TEnum defaultValue)
        where TEnum : struct
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;

            return Enum.TryParse<TEnum>(value, out var e) ? e : defaultValue;
        }
        catch (ArgumentException)
        {
            return defaultValue;
        }
    }

    public static void SetInvokeMember(this MemberInfo info, object instance, params object?[]? parameter)
    {
        if (info is null) throw new ArgumentNullException(nameof(info));

        switch (info)
        {
            case PropertyInfo property:
            {
                SetProperty(instance, parameter, property);
                break;
            }
            case FieldInfo field:
            {
                SetField(instance, parameter, field);
                break;
            }
            case MethodInfo method:
                method.InvokeFast(instance, parameter ?? Array.Empty<object>());

                break;
        }
    }

    private static void SetField(object instance, object?[]? parameter, FieldInfo field)
    {
        object? value = null;
        if (parameter != null) value = parameter.FirstOrDefault();

        FastReflection.Shared.GetFieldSetter(field)(instance, value);
    }

    private static void SetProperty(object instance, object?[]? parameter, PropertyInfo property)
    {
        object? value = null;
        object?[]? indexes = null;
        if (parameter != null)
        {
            if (parameter.Length >= 1) value = parameter[0];
            if (parameter.Length > 1) indexes = parameter.Skip(1).ToArray();
        }

        FastReflection.Shared.GetPropertySetter(property)(instance, indexes, value);
    }

    public static bool TryParseEnum<TEnum>(this string value, out TEnum eEnum) where TEnum : struct
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        return Enum.TryParse(value, out eEnum);
    }

    public static void SetFieldFast(this FieldInfo field, object target, object? value)
        => FastReflection.Shared.GetFieldSetter(field)(target, value);

    public static void SetValueFast(this PropertyInfo info, object target, object? value, params object[] index)
        => FastReflection.Shared.GetPropertySetter(info)(target, index, value);

    public static object FastCreate(this ConstructorInfo info, params object[] parms)
        => FastReflection.Shared.GetCreator(info)(parms);

    public static object? GetValueFast(this PropertyInfo info, object? instance, params object[] index)
        => FastReflection.Shared.GetPropertyAccessor(
            info,
            () => info.GetIndexParameters().Select(pi => pi.ParameterType)).Invoke(instance, index);

    public static object? GetValueFast(this FieldInfo info, object? instance)
        => FastReflection.Shared.GetFieldAccessor(info)(instance);
}