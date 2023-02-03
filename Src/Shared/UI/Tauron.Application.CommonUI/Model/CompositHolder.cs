using System;
using System.Reactive.Disposables;

namespace Tauron.Application.CommonUI.Model;

public sealed class CompositHolder : IResourceHolder
{
    private readonly CompositeDisposable _composite = new();

    public void Dispose()
        => _composite.Dispose();

    public void AddResource(IDisposable res)
        => _composite.Add(res);

    public void RemoveResource(IDisposable res)
        => _composite.Remove(res);
}