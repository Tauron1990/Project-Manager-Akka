using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Akka.Util;
using JetBrains.Annotations;

namespace Tauron.Application.CommonUI.Helper
{
    [PublicAPI]
    public sealed class WeakDelegate : IInternalWeakReference, IEquatable<WeakDelegate>
    {
        private readonly MethodInfo _method;

        private readonly WeakReference? _reference;

        public WeakDelegate(Delegate @delegate)
        {
            _method = @delegate.Method;

            if (!_method.IsStatic) _reference = new WeakReference(@delegate.Target);
        }

        public WeakDelegate(MethodInfo methodInfo, object target)
        {
            _method = methodInfo;
            _reference = new WeakReference(target);
        }

        public bool Equals(WeakDelegate? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return other._reference?.Target == _reference?.Target && other._method == _method;
        }

        public bool IsAlive => _reference == null || _reference.IsAlive;

        public static bool operator ==(WeakDelegate? left, WeakDelegate? right)
        {
            var leftnull = ReferenceEquals(left, null);
            var rightNull = ReferenceEquals(right, null);

            return !leftnull ? left!.Equals(right!) : rightNull;
        }

        public static bool operator !=(WeakDelegate? left, WeakDelegate? right)
        {
            var leftnull = ReferenceEquals(left, null);
            var rightNull = ReferenceEquals(right, null);

            if (!leftnull) return !left!.Equals(right!);

            return !rightNull;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj is WeakDelegate @delegate && Equals(@delegate);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var target = _reference?.Target;

                return ((target != null ? target.GetHashCode() : 0) * 397)
                     ^ _method.GetHashCode();
            }
        }

        public Option<object> Invoke(params object[] parms)
        {
            if (_method.IsStatic)
                return _method.GetMethodInvoker(() => _method.GetParameterTypes()).Invoke(null, parms).OptionNotNull();

            var target = _reference?.Target;

            return target == null
                ? Option<object>.None
                : _method.GetMethodInvoker(() => _method.GetParameterTypes()).Invoke(target, parms).OptionNotNull();
        }
    }

    [PublicAPI]
    public static class WeakCleanUp
    {
        //public const string WeakCleanUpExceptionPolicy = "WeakCleanUpExceptionPolicy";

        private static readonly List<WeakDelegate> Actions = Initialize();

        private static Timer? _timer;

        public static void RegisterAction(Action action)
        {
            lock (Actions)
            {
                Actions.Add(new WeakDelegate(action));
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
                    switch (weakDelegate.IsAlive)
                    {
                        case true:
                            try
                            {
                                #pragma warning disable GU0011
                                weakDelegate.Invoke();
                                #pragma warning restore GU0011
                            }
                            catch (Exception e)
                            {
                                if (e.IsCriticalException()) throw;
                            }

                            break;
                        default:
                            dead.Add(weakDelegate);

                            break;
                    }

                dead.ForEach(del => Actions.Remove(del));
            }
        }
    }
}