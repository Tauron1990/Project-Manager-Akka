using System;
using System.Reactive.Disposables;
using ReactiveUI;
using Tauron;

namespace SimpleProjectManager.Client.Shared.ViewModels
{
    public abstract class ViewModelBase : ReactiveObject, IActivatableViewModel, IDisposable
    {
        protected readonly CompositeDisposable Disposer = new();

        public ViewModelActivator Activator { get; } = new();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            Disposer.Dispose();
            Activator.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}