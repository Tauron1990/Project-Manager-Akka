using System;
using System.Reactive.Disposables;
using Microsoft.AspNetCore.Components;
using Tauron.Akka;

namespace Tauron.Application.Blazor
{
    public abstract class DispoableComponent : ComponentBase, IResourceHolder, ICancelable
    {
        private readonly CompositeDisposable _disposer = new();

        public bool IsDisposed => _disposer.IsDisposed;

        public void Dispose() => _disposer.Dispose();

        void IResourceHolder.AddResource(IDisposable res) => _disposer.Add(res);

        void IResourceHolder.RemoveResource(IDisposable res) => _disposer.Remove(res);
    }
}