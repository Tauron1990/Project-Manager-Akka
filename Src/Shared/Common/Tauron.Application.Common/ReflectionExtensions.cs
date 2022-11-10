using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Stl;

namespace Tauron;

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
        if(type == null) throw new ArgumentNullException(nameof(type));

        bindingflags |= BindingFlags.Public;
        if(nonPublic) bindingflags |= BindingFlags.NonPublic;

        if(!Enum.IsDefined(typeof(BindingFlags), BindingFlags.FlattenHierarchy))
            return from mem in type.GetMembers(bindingflags)
                   let attr = CustomAttributeExtensions.GetCustomAttribute<TAttribute>(mem)
                   where attr != null
                   select (mem, attr);

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
        Type? targetType = type;
        while (targetType != null)
        {
            foreach (MemberInfo mem in targetType.GetMembers(flags)) yield return mem;

            targetType = targetType.BaseType;
        }
    }

    public static IEnumerable<(MemberInfo Member, TAttribute Attribute)> FindMemberAttributes<TAttribute>(
        this Type type,
        bool nonPublic) where TAttribute : Attribute
        => FindMemberAttributes<TAttribute>(type, nonPublic, BindingFlags.Instance | BindingFlags.FlattenHierarchy);

    public static T[] GetAllCustomAttributes<T>(this ICustomAttributeProvider member) where T : class
        => (T[])member.GetCustomAttributes(typeof(T), true);


    public static object[] GetAllCustomAttributes(this ICustomAttributeProvider member, Type type)
        => member.GetCustomAttributes(type, true);

    public static Option<TAttribute> GetCustomAttribute<TAttribute>(this ICustomAttributeProvider provider)
        where TAttribute : Attribute
        => GetCustomAttribute<TAttribute>(provider, true);

    public static Option<TAttribute> GetCustomAttribute<TAttribute>(this ICustomAttributeProvider provider, bool inherit)
        where TAttribute : Attribute
    {
        object? temp = provider.GetCustomAttributes(typeof(TAttribute), inherit).FirstOrDefault();

        return (temp as TAttribute).OptionNotNull();
    }

    public static IEnumerable<object> GetCustomAttributes(this ICustomAttributeProvider provider, params Type[] attributeTypes)
    {
        if(provider == null) throw new ArgumentNullException(nameof(provider));

        return from attributeType in attributeTypes
               from attribute in provider.GetCustomAttributes(attributeType, false)
               select attribute;
    }

    public static Option<TType> GetInvokeMember<TType>(this MemberInfo info, object instance, params object[]? parameter)
    {
        if(info == null) throw new ArgumentNullException(nameof(info));

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
        if(method == null) throw new ArgumentNullException(nameof(method));

        var mi = method as MethodInfo;

        if(mi != null && mi.IsGenericMethod) return mi.GetGenericMethodDefinition().MethodHandle;

        return method.MethodHandle;
    }

    public static IEnumerable<Type> GetParameterTypes(this MethodBase method)
    {
        if(method is null) throw new ArgumentNullException(nameof(method));

        return method.GetParameters().Select(p => p.ParameterType);
    }

    public static Option<PropertyInfo> GetPropertyFromMethod(this MethodInfo method, Type implementingType)
    {
        if(method is null) throw new ArgumentNullException(nameof(method));
        if(implementingType is null) throw new ArgumentNullException(nameof(implementingType));

        if(!method.IsSpecialName || method.Name.Length < 4) return default;

        bool isGetMethod = method.Name.Substring(0, 4) == "get_";
        Type returnType = isGetMethod ? method.ReturnType : method.GetParameterTypes().Last();
        var indexerTypes = isGetMethod
            ? method.GetParameterTypes()
            : method.GetParameterTypes().SkipLast(1);

        return implementingType.GetProperty(method.Name[4..], DefaultBindingFlags, null, returnType, indexerTypes.ToArray(), null)
           .OptionNotNull();
    }

    public static Option<PropertyInfo> GetPropertyFromMethod(this MethodBase method)
    {
        if(method == null) throw new ArgumentNullException(nameof(method));

        return !method.IsSpecialName
            ? default
            : (method.DeclaringType?.GetProperty(method.Name[4..], DefaultBindingFlags)).OptionNotNull();
    }

    public static Type GetSetInvokeType(this MemberInfo info)
    {
        if(info == null) throw new ArgumentNullException(nameof(info));

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
        if(member == null) throw new ArgumentNullException(nameof(member));

        return member.IsDefined(typeof(T), true);
    }

    public static bool HasAttribute(this ICustomAttributeProvider member, Type type)
    {
        if(member == null) throw new ArgumentNullException(nameof(member));
        if(type == null) throw new ArgumentNullException(nameof(type));

        return member.IsDefined(type, true);
    }

    public static bool HasMatchingAttribute<T>(this ICustomAttributeProvider member, T attributeToMatch)
        where T : Attribute
    {
        if(member == null) throw new ArgumentNullException(nameof(member));
        if(attributeToMatch == null) throw new ArgumentNullException(nameof(attributeToMatch));

        var attributes = member.GetAllCustomAttributes<T>();

        return attributes.Length != 0 && attributes.Any(attribute => attribute.Match(attributeToMatch));
    }

    public static Option<TType> InvokeFast<TType>(this MethodBase method, object? instance, params object?[] args)
    {
        if(method == null) throw new ArgumentNullException(nameof(method));

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
        if(value == null) throw new ArgumentNullException(nameof(value));

        return (TEnum)Enum.Parse(typeof(TEnum), value);
    }

    public static TEnum TryParseEnum<TEnum>(this string value, TEnum defaultValue)
        where TEnum : struct
    {
        try
        {
            if(string.IsNullOrWhiteSpace(value)) return defaultValue;

            return Enum.TryParse<TEnum>(value, out TEnum e) ? e : defaultValue;
        }
        catch (ArgumentException)
        {
            return defaultValue;
        }
    }

    public static void SetInvokeMember(this MemberInfo info, object instance, params object?[]? parameter)
    {
        if(info is null) throw new ArgumentNullException(nameof(info));

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
        if(parameter != null) value = parameter.FirstOrDefault();

        FastReflection.Shared.GetFieldSetter(field)(instance, value);
    }

    private static void SetProperty(object instance, object?[]? parameter, PropertyInfo property)
    {
        object? value = null;
        object?[]? indexes = null;
        if(parameter != null)
        {
            if(parameter.Length >= 1) value = parameter[0];
            if(parameter.Length > 1) indexes = parameter.Skip(1).ToArray();
        }

        FastReflection.Shared.GetPropertySetter(property)(instance, indexes, value);
    }

    public static bool TryParseEnum<TEnum>(this string value, out TEnum eEnum) where TEnum : struct
    {
        if(value == null) throw new ArgumentNullException(nameof(value));

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