using System;
using System.Reactive.Disposables;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Stl.Fusion.Blazor;

namespace Tauron.Application.Blazor;

[PublicAPI]
public abstract class DisposableComponent : ComponentBase, IResourceHolder
{
    private readonly CompositeDisposable _disposables = new();

    public void AddResource(IDisposable disposable) => _disposables.Add(disposable);

    public void RemoveResource(IDisposable res) => _disposables.Remove(res);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if(disposing)
            _disposables.Dispose();
    }
}

public abstract class DisposableComponent<TState> : ComputedStateComponent<TState>, IResourceHolder
{
    private readonly CompositeDisposable _disposables = new();

    public void AddResource(IDisposable disposable) => _disposables.Add(disposable);

    public void RemoveResource(IDisposable res) => _disposables.Remove(res);

    public void Dispose()
    {
        _disposables.Dispose();
    }
}