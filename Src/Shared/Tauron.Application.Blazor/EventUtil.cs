using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;

namespace Tauron.Application.Blazor;

[PublicAPI]
public static class EventUtil
{
    // The repetition in here is because of the four combinations of handlers (sync/async * with/without arg)
    public static Action AsNonRenderingEventHandler(Action callback)
        => new SyncReceiver(callback).Invoke;
    public static Action<TValue> AsNonRenderingEventHandler<TValue>(Action<TValue> callback)
        => new SyncReceiver<TValue>(callback).Invoke;
    public static Func<Task> AsNonRenderingEventHandler(Func<Task> callback)
        => new AsyncReceiver(callback).Invoke;
    public static Func<TValue, Task> AsNonRenderingEventHandler<TValue>(Func<TValue, Task> callback)
        => new AsyncReceiver<TValue>(callback).Invoke;

    record SyncReceiver(Action Callback) : ReceiverBase { internal void Invoke() => Callback(); }
    record SyncReceiver<T>(Action<T> Callback) : ReceiverBase { internal void Invoke(T arg) => Callback(arg); }
    record AsyncReceiver(Func<Task> Callback) : ReceiverBase { internal Task Invoke() => Callback(); }
    record AsyncReceiver<T>(Func<T, Task> Callback) : ReceiverBase { internal Task Invoke(T arg) => Callback(arg); }

    // By implementing IHandleEvent, we can override the event handling logic on a per-handler basis
    // The logic here just calls the callback without triggering any re-rendering
    record ReceiverBase : IHandleEvent
    {
        public Task HandleEventAsync(EventCallbackWorkItem item, object? arg) => item.InvokeAsync(arg);
    }
}