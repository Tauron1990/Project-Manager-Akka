using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Components;

namespace ServiceManager.Client.Components
{
    public abstract class DisposableComponent : ComponentBase, IDisposable
    {
        private List<IDisposable>? _disposables = new();

        public void AddResource(IDisposable disposable)
        {
            if(_disposables == null)
                disposable.Dispose();
            else
                _disposables.Add(disposable);
        }

        public void RemoveResource(IDisposable disposable)
        {
            if (_disposables == null)
                disposable.Dispose();
            else
                _disposables.Remove(disposable);
        }

        public virtual void Dispose()
        {
            if(_disposables == null) return;
            foreach (var disposable in Interlocked.Exchange(ref _disposables, null)) 
                disposable.Dispose();
        }
    }
}