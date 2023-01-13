using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;

namespace Tauron.Application;

[PublicAPI]
public static class WeakCleanUp
{
    public const string ExceptionPolicy = "WeakCleanUpExceptionPolicy";

    private static readonly IList<WeakDelegate> Actions = Initialize();

    #pragma warning disable IDE0052 // Ungelesene private Member entfernen
    private static Timer? _timer;
    #pragma warning restore IDE0052 // Ungelesene private Member entfernen

    public static void RegisterAction(Action action)
    {
        lock (Actions)
        {
            Actions.Add(new WeakDelegate(action));
        }
    }

    private static IList<WeakDelegate> Initialize()
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
                if(weakDelegate.IsAlive)
                    try
                    {
                        #pragma warning disable GU0011
                        weakDelegate.Invoke();
                        #pragma warning restore GU0011
                    }
                    catch (ApplicationException) { }
                else
                    dead.Add(weakDelegate);

            dead.ForEach(del => Actions.Remove(del));
        }
    }
}