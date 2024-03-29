﻿using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Tauron;
using Tauron.Application;

namespace ServiceManager.Server.AppCore
{
    public interface IResource<TData>
    {
        Task<TData> Init(Func<Task<TData>> init, CancellationToken token = default);

        Task<Unit> Set(Func<Task<TData>> data, CancellationToken token = default);

        Task<Unit> Set(Func<TData, TData> data, CancellationToken token = default);

        Task<Unit> Set(Func<TData> data, CancellationToken token = default);

        Task<Unit> Set(TData data, CancellationToken token = default);

        TData Get();
    }

    public abstract class ResourceHoldingObject : ObservableObject, IResourceHolder
    {
        private readonly CompositeDisposable _disposable = new();
        private readonly SemaphoreSlim _resourceLock = new(1);

        protected ResourceHoldingObject() => _disposable.Add(_resourceLock);

        public virtual void Dispose() => _disposable.Dispose();

        void IResourceHolder.AddResource(IDisposable res) => _disposable.Add(res);

        void IResourceHolder.RemoveResource(IDisposable res) => _disposable.Remove(res);

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

        private sealed class InternalResource<TData> : IResource<TData>
        {
            private readonly Func<Task>? _onChange;
            private readonly SemaphoreSlim _resourceLock;
            private TData _data;
            private bool _isSet;

            internal InternalResource(TData value, SemaphoreSlim resourceLock, Func<Task>? onChange)
            {
                _data = value;
                _resourceLock = resourceLock;
                _onChange = onChange;
            }

            public async Task<TData> Init(Func<Task<TData>> init, CancellationToken token = default)
            {
                await _resourceLock.WaitAsync(token);
                try
                {
                    if (_isSet) return _data;

                    var result = await init();
                    _data = result;
                    _isSet = true;
                    if (_onChange != null)
                        await _onChange.Invoke();

                    return result;
                }
                finally
                {
                    _resourceLock.Release();
                }
            }

            public async Task<Unit> Set(TData data, CancellationToken token = default)
            {
                await _resourceLock.WaitAsync(token);
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

            public async Task<Unit> Set(Func<Task<TData>> data, CancellationToken token = default)
            {
                await _resourceLock.WaitAsync(token);
                try
                {
                    _data = await data();
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

            public async Task<Unit> Set(Func<TData, TData> data, CancellationToken token = default)
            {
                await _resourceLock.WaitAsync(token);
                try
                {
                    _data = data(_data);
                    _isSet = true;
                    if (_onChange != null)
                        await _onChange();

                    return Unit.Default;
                }
                finally
                {
                    _resourceLock.Release();
                }
            }

            public Task<Unit> Set(Func<TData> data, CancellationToken token = default) => Set(data(), token);
        }
    }
}