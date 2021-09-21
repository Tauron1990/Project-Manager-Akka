using System;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace ServiceManager.Client.Components.Operations
{
    public sealed class OperationManager : IOperationManager, IDisposable
    {
        public static readonly IOperationManager Empty = new OperationManager(null);

        private readonly Subject<bool>? _informer;

        private OperationManager(Subject<bool>? informer) => _informer = informer;

        public OperationManager() => _informer = new Subject<bool>();

        public void Dispose()
        {
            if (_informer == null) return;

            _informer.OnCompleted();
            _informer.Dispose();
        }

        public IDisposable Start()
        {
            if (_informer == null)
                return Disposable.Empty;

            _informer.OnNext(true);

            return Disposable.Create(() => _informer.OnNext(false));
        }

        public IDisposable Subscribe(IObserver<bool> action)
            => _informer == null ? Disposable.Empty : _informer.Subscribe(action);
    }
}