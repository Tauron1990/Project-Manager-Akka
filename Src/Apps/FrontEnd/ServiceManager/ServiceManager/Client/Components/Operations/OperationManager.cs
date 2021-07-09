using System;
using System.Reactive.Subjects;

namespace ServiceManager.Client.Components.Operations
{
    public sealed class OperationManager : IOperationManager, IDisposable
    {
        private readonly Subject<bool> _informer = new();

        public IDisposable Start()
        {
            _informer.OnNext(true);
            return Disposables.Create(() => _informer.OnNext(false));
        }

        public IDisposable Subscribe(IObserver<bool> action)
            => _informer.Subscribe(action);

        public void Dispose()
        {
            _informer.OnCompleted();
            _informer.Dispose();
        }
    }
}