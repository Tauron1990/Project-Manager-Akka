using System;
using System.Reactive.Disposables;
using Microsoft.AspNetCore.Components;
using Tauron.Akka;

namespace Tauron.Application.ServiceManager.Components
{
    public abstract class DisposingComponent : ComponentBase, IDisposable, IResourceHolder
    {
        public CompositeDisposable Disposer { get; } = new();

        public void Dispose() => Disposer.Dispose();

        void IResourceHolder.AddResource(IDisposable res)
            => Disposer.Add(res);

        void IResourceHolder.RemoveResources(IDisposable res)
            => Disposer.Remove(res);
    }
}