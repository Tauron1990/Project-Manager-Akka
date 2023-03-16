using System;
using System.Reflection;

namespace Tauron.Application;

[PublicAPI]
public sealed class WeakDelegate : IWeakReference, IEquatable<WeakDelegate>
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
        if(other is null) return false;
        if(ReferenceEquals(this, other)) return true;

        return other._reference?.Target == _reference?.Target && other._method == _method;
    }

    public bool IsAlive => _reference == null || _reference.IsAlive;

    public static bool operator ==(WeakDelegate? left, WeakDelegate? right)
    {
        bool rightNull = right is null;

        return left?.Equals(right) ?? rightNull;
    }

    public static bool operator !=(WeakDelegate? left, WeakDelegate? right)
    {
        bool rightNull = right is null;

        if(left is not null) return !left.Equals(right);

        return !rightNull;
    }

    public override bool Equals(object? obj)
    {
        if(obj is null) return false;
        if(ReferenceEquals(this, obj)) return true;

        return obj is WeakDelegate @delegate && Equals(@delegate);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            object? target;

            return (((target = _reference?.Target) != null ? target.GetHashCode() : 0) * 397)
                 ^ _method.GetHashCode();
        }
    }

    public object? Invoke(params object[]? parms)
    {
        if(_method.IsStatic)
            return _method.GetMethodInvoker(() => _method.GetParameterTypes()).Invoke(null, parms);

        object? target = _reference?.Target;

        return target == null
            ? null
            : _method.GetMethodInvoker(() => _method.GetParameterTypes()).Invoke(target, parms);
    }
}