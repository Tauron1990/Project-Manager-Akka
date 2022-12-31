using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;

namespace Tauron.Application.CommonUI.Helper;

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
        _timer = new Timer(InvokeCleanUp, state: null, TimeSpan.Zero, TimeSpan.FromMinutes(15));

        return new List<WeakDelegate>();
    }

    private static void InvokeCleanUp(object? state)
    {
        lock (Actions)
        {
            var dead = new List<WeakDelegate>();
            foreach (var weakDelegate in Actions.ToArray())
                InvokeDelegate(weakDelegate, dead);

            dead.ForEach(del => Actions.Remove(del));
        }
    }

    private static void InvokeDelegate(WeakDelegate weakDelegate, List<WeakDelegate> dead)
    {
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
                    if(e.IsCriticalException()) throw;
                }

                break;
            default:
                dead.Add(weakDelegate);

                break;
        }
    }
}