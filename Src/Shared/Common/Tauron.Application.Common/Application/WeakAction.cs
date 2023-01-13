using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Tauron.Application;

[PublicAPI]
public sealed class WeakAction
{
    private readonly Type?[] _parames;

    public WeakAction(object? target, MethodInfo method, Type? parameterType)
    {
        if(target != null)
            TargetObject = new WeakReference(target);

        MethodInfo = method;
        _parames = new[] { parameterType };

        //_delegateType = parameterType == null
        //    ? typeof(Action)
        //    : typeof(Action<>).MakeGenericType(parameterType);

        ParameterCount = parameterType == null ? 0 : 1;
    }

    public WeakAction(object? target, MethodInfo method)
    {
        MethodInfo = method;
        if(target != null)
            TargetObject = new WeakReference(target);

        var parames = method.GetParameters().OrderBy(parm => parm.Position).Select(parm => parm.ParameterType)
           .ToArray();
        _parames = parames;

        //var returntype = method.ReturnType;
        //_delegateType = returntype == typeof(void)
        //    ? FactoryDelegateType("System.Action", parames.ToArray())
        //    : FactoryDelegateType("System.Func", parames.Concat(new[] {returntype}).ToArray());

        ParameterCount = parames.Length;
    }

    public int ParameterCount { get; private set; }

    public MethodInfo MethodInfo { get; }

    public WeakReference? TargetObject { get; }

    private bool Equals(WeakAction other) => Equals(MethodInfo, other.MethodInfo) &&
                                             Equals(TargetObject?.Target, other.TargetObject?.Target);

    public override int GetHashCode()
    {
        unchecked
        {
            int? value = TargetObject?.Target?.GetHashCode();

            return (MethodInfo.GetHashCode() * 397) ^ (value is null ? 0 : value.Value);
        }
    }

    public object? Invoke(params object[] parms)
    {
        var temp = CreateDelegate(out object? target);

        return temp?.Invoke(target, parms);
    }

    public override bool Equals(object? obj)
    {
        if(obj is null) return false;
        if(ReferenceEquals(this, obj)) return true;

        return obj is WeakAction action && Equals(action);
    }

    internal Func<object?, object?[]?, object?>? CreateDelegate(out object? target)
    {
        target = TargetObject?.Target;

        return target != null
            ? MethodInfo.GetMethodInvoker(() => _parames)
            : null;
    }

    //[NotNull]
    //private static Type FactoryDelegateType([NotNull] string name, Type[] types)
    //{
    //    Argument.NotNull(types, nameof(types));
    //    Argument.NotNull(name, nameof(name));

    //    var type = Type.GetType(name + "`" + types.Length);
    //    if (type != null)
    //        return types.Length > 0 ? type.MakeGenericType(types) : Argument.CheckResult(Type.GetType(name), "Delegate Type Was Null");

    //    throw new InvalidOperationException();
    //}
}