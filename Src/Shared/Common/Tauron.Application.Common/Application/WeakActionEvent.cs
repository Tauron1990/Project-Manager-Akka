using System;
using System.Collections.Generic;
using System.Linq;

namespace Tauron.Application;

[PublicAPI]
public class WeakActionEvent<T>
{
    private readonly List<WeakAction> _delegates = new();
    private readonly object _lock = new();

    public WeakActionEvent()
    {
        WeakCleanUp.RegisterAction(CleanUp);
    }

    private void CleanUp()
    {
        lock (_lock)
        {
            var dead = _delegates.Where(item => item.TargetObject?.IsAlive == false).ToList();

            lock (_lock)
            {
                dead.ForEach(ac => _delegates.Remove(ac));
            }
        }
    }

    public WeakActionEvent<T> Add(Action<T> handler)
    {
        var parameters = handler.Method.GetParameters();

        lock (_lock)
        {
            if(_delegates.Where(del => del.MethodInfo == handler.Method)
              .Select(weakAction => weakAction.TargetObject?.Target)
              .Any(weakTarget => weakTarget == handler.Target))
                return this;
        }

        Type parameterType = parameters[0].ParameterType;

        lock (_lock)
        {
            _delegates.Add(new WeakAction(handler.Target, handler.Method, parameterType));
        }

        return this;
    }

    public void Invoke(T arg)
    {
        lock (_lock)
        {
            foreach (WeakAction action in _delegates)
            {
                var del = action.CreateDelegate(out object? target);
                if(target != null)
                    del?.Invoke(target, new object?[] { arg });
            }
        }
    }

    public WeakActionEvent<T> Remove(Action<T> handler)
    {
        lock (_lock)
        {
            foreach (WeakAction del in _delegates.Where(
                         del
                             => del.TargetObject != null && del.TargetObject.Target == handler.Target))
            {
                _delegates.Remove(del);

                return this;
            }
        }

        return this;
    }
}