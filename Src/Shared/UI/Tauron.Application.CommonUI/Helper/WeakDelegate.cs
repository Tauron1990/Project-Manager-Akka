using System;
using System.Reflection;
using Akka.Util;
using JetBrains.Annotations;

namespace Tauron.Application.CommonUI.Helper;

[PublicAPI]
public sealed class WeakDelegate : IInternalWeakReference, IEquatable<WeakDelegate>
{
    private readonly MethodInfo _method;

    private readonly WeakReference? _reference;

    public WeakDelegate(Delegate @delegate)
    {
        _method = @delegate.Method;

        if(!_method.IsStatic) _reference = new WeakReference(@delegate.Target);
    }

    public WeakDelegate(MethodInfo methodInfo, object target)
    {
        _method = methodInfo;
        _reference = new WeakReference(target);
    }

    public bool Equals(WeakDelegate? other)
    {
        if(ReferenceEquals(null, other)) return false;
        if(ReferenceEquals(this, other)) return true;

        return other._reference?.Target == _reference?.Target && other._method == _method;
    }

    public bool IsAlive => _reference is null || _reference.IsAlive;

    public static bool operator ==(WeakDelegate? left, WeakDelegate? right)
    {
        bool leftnull = ReferenceEquals(left, null);
        bool rightNull = ReferenceEquals(right, null);

        return !leftnull ? left!.Equals(right!) : rightNull;
    }

    public static bool operator !=(WeakDelegate? left, WeakDelegate? right)
    {
        bool leftnull = ReferenceEquals(left, null);
        bool rightNull = ReferenceEquals(right, null);

        if(!leftnull) return !left!.Equals(right!);

        return !rightNull;
    }

    public override bool Equals(object? obj)
    {
        if(ReferenceEquals(null, obj)) return false;
        if(ReferenceEquals(this, obj)) return true;

        return obj is WeakDelegate @delegate && Equals(@delegate);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            object? target = _reference?.Target;

            return ((target != null ? target.GetHashCode() : 0) * 397)
                 ^ _method.GetHashCode();
        }
    }

    public Option<object> Invoke(params object[] parms)
    {
        if(_method.IsStatic)
            return _method.GetMethodInvoker(() => _method.GetParameterTypes()).Invoke(arg1: null, parms).OptionNotNull();

        object? target = _reference?.Target;

        return target is null
            ? Option<object>.None
            : _method.GetMethodInvoker(() => _method.GetParameterTypes()).Invoke(target, parms).OptionNotNull();
    }
}