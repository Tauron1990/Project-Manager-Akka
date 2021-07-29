using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Tauron.Akka;
using Tauron.Application;

namespace ServiceManager.Server.AppCore
{
    public interface IResource<TData>
    {
        Task<TData> Init(Func<Task<TData>> init);

        Task<Unit> Set(Func<Task<TData>> data);

        Task<Unit> Set(Func<TData> data);

        Task<Unit> Set(TData data);

        TData Get();
    }

    public class ResourceHoldingObject : ObservableObject, IDisposable, IResourceHolder
    {
        private readonly SemaphoreSlim _resourceLock = new(1);
        private readonly CompositeDisposable _disposable = new();

        public ResourceHoldingObject() => _disposable.Add(_resourceLock);

        protected IResource<TData> CreateResource<TData>(TData value, string? name = default, Func<Task>? onSet = default)
        {
            var setter = string.IsNullOrWhiteSpace(name) switch
            {
                true when onSet != null => onSet,
                false when onSet != null => async () =>
                                            {
                                                OnPropertyChangedExplicit(name!);
                                                await onSet();
                                            },
                false when onSet == null => () =>
                                            {
                                                OnPropertyChangedExplicit(name!);
                                                return Task.CompletedTask;
                                            },
                _ => null
            };

            return new InternalResource<TData>(value, _resourceLock, setter);
        }

        public void Dispose() => _disposable.Dispose();

        void IResourceHolder.AddResource(IDisposable res) => _disposable.Add(res);

        void IResourceHolder.RemoveResource(IDisposable res) => _disposable.Remove(res);

        private sealed class InternalResource<TData> : IResource<TData>
        {
            private readonly SemaphoreSlim _resourceLock;
            private readonly Func<Task>? _onChange;
            private bool _isSet;
            private TData _data;

            public InternalResource(TData value, SemaphoreSlim resourceLock, Func<Task>? onChange)
            {
                _data = value;
                _resourceLock = resourceLock;
                _onChange = onChange;
            }

            public async Task<TData> Init(Func<Task<TData>> init)
            {
                await _resourceLock.WaitAsync();
                try
                {
                    if (_isSet) return _data;

                    var result = await init();
                    _data = result;
                    _isSet = true;
                    if(_onChange != null)
                        await _onChange.Invoke();

                    return result;
                }
                finally
                {
                    _resourceLock.Release();
                }
            }

            public async Task<Unit> Set(TData data)
            {
                await _resourceLock.WaitAsync();
                try
                {
                    _data = data;
                    _isSet = true;
                    if (_onChange != null)
                        await _onChange.Invoke();
                }
                finally
                {
                    _resourceLock.Release();
                }

                return Unit.Default;
            }

            public TData Get()
            {
                _resourceLock.Wait();
                try
                {
                    return _data;
                }
                finally
                {
                    _resourceLock.Release();
                }
            }

            public async Task<Unit> Set(Func<Task<TData>> data)
            {
                await _resourceLock.WaitAsync();
                try
                {
                    _data = await data();
                    _isSet = true;
                    if(_onChange != null)
                        await _onChange.Invoke();
                }
                finally
                {
                    _resourceLock.Release();
                }

                return Unit.Default;
            }

            public Task<Unit> Set(Func<TData> data) => Set(data());
        }

    }
}