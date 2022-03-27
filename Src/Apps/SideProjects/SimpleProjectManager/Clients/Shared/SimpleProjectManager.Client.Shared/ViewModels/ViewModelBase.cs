using System;
using System.Reactive.Disposables;
using ReactiveUI;
using Tauron;

namespace SimpleProjectManager.Client.Shared.ViewModels
{
    public abstract class ViewModelBase : ReactiveObject, IResourceHolder, IActivatableViewModel
    {
        private readonly CompositeDisposable _disposable = new();

        protected ViewModelBase()
            => this.WhenActivated(dispo => dispo(_disposable));

        void IDisposable.Dispose()
        {
            _disposable.Dispose();
            GC.SuppressFinalize(this);
        }

        void IResourceHolder.AddResource(IDisposable res)
            => _disposable.Add(res);

        void IResourceHolder.RemoveResource(IDisposable res)
            => _disposable.Add(res);

        public ViewModelActivator Activator { get; } = new();
    }
}