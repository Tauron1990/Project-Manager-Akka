using System;
using System.Reactive.Disposables;
using Microsoft.AspNetCore.Components;
using Stl.Fusion.Blazor;
using Tauron.Akka;

namespace ServiceManager.Client.Components
{
    public abstract class DisposableComponent : ComponentBase, IResourceHolder
    {
        private readonly CompositeDisposable _disposables = new();

        public void AddResource(IDisposable disposable) => _disposables.Add(disposable);

        public void RemoveResource(IDisposable res) => _disposables.Remove(res);

        public virtual void Dispose() => _disposables.Dispose();
    }

    public abstract class DisposableComponent<TState> : ComputedStateComponent<TState>, IResourceHolder
    {
        private readonly CompositeDisposable _disposables = new();

        public void AddResource(IDisposable disposable) => _disposables.Add(disposable);

        public void RemoveResource(IDisposable res) => _disposables.Remove(res);

        public override void Dispose()
        {
            _disposables.Dispose();
            base.Dispose();
        }
    }
}