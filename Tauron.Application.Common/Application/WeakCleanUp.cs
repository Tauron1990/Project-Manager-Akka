﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;

namespace Tauron.Application
{
    [PublicAPI]
    public sealed class WeakDelegate : IWeakReference, IEquatable<WeakDelegate>
    {
        private readonly MethodInfo _method;

        private readonly WeakReference? _reference;

        public WeakDelegate(Delegate @delegate)
        {
            Argument.NotNull(@delegate, nameof(@delegate));

            _method = @delegate.Method;

            if (!_method.IsStatic) _reference = new WeakReference(@delegate.Target);
        }

        public WeakDelegate(MethodInfo methodInfo, object target)
        {
            _method = Argument.NotNull(methodInfo, nameof(methodInfo));
            _reference = new WeakReference(Argument.NotNull(target, nameof(target)));
        }

        public bool Equals(WeakDelegate? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return other._reference?.Target == _reference?.Target && other._method == _method;
        }

        public bool IsAlive => _reference == null || _reference.IsAlive;

        public static bool operator ==(WeakDelegate? left, WeakDelegate? right)
        {
            var rightNull = right is null;

            return left?.Equals(right) ?? rightNull;
        }

        public static bool operator !=(WeakDelegate left, WeakDelegate right)
        {
            var rightNull = right is null;

            if (left is not null) return !left.Equals(right);
            return !rightNull;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;

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
            if (_method.IsStatic) return _method.GetMethodInvoker(() => _method.GetParameterTypes()).Invoke(null, parms);

            var target = _reference?.Target;
            return target == null ? null : _method.GetMethodInvoker(() => _method.GetParameterTypes()).Invoke(target, parms);
        }
    }

    [PublicAPI]
    public static class WeakCleanUp
    {
        public const string WeakCleanUpExceptionPolicy = "WeakCleanUpExceptionPolicy";

        private static readonly List<WeakDelegate> Actions = Initialize();

#pragma warning disable IDE0052 // Ungelesene private Member entfernen
        private static Timer? _timer;
#pragma warning restore IDE0052 // Ungelesene private Member entfernen

        public static void RegisterAction([NotNull] Action action)
        {
            lock (Actions)
            {
                Actions.Add(new WeakDelegate(Argument.NotNull(action, nameof(action))));
            }
        }

        private static List<WeakDelegate> Initialize()
        {
            _timer = new Timer(InvokeCleanUp, null, TimeSpan.Zero, TimeSpan.FromMinutes(15));
            return new List<WeakDelegate>();
        }

        private static void InvokeCleanUp(object? state)
        {
            lock (Actions)
            {
                var dead = new List<WeakDelegate>();
                foreach (var weakDelegate in Actions.ToArray())
                {
                    if (weakDelegate.IsAlive)
                    {
                        try
                        {
                            weakDelegate.Invoke();
                        }
                        catch (ApplicationException)
                        {
                        }
                    }
                    else
                        dead.Add(weakDelegate);
                }

                dead.ForEach(del => Actions.Remove(del));
            }
        }
    }
}