using Microsoft.AspNetCore.Components;
using SimpleProjectManager.Client.Shared.Services;

namespace SimpleProjectManager.Client.Core;

public readonly struct EventCallBackImpl<TEvt> : IEventCallback<TEvt>
{
    private readonly EventCallback<TEvt> _eventCallback;

    public EventCallBackImpl(EventCallback<TEvt> eventCallback)
        => _eventCallback = eventCallback;

    #pragma warning disable EPS06

    public Task InvokeAsync(TEvt parameter)
        => _eventCallback.InvokeAsync(parameter);
}

public readonly struct EventCallBackImpl : IEventCallback
{
    private readonly EventCallback _callback;

    public EventCallBackImpl(EventCallback callback)
        => _callback = callback;

    public Task InvokeAsync()
        => _callback.InvokeAsync();
}