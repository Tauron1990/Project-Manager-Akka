using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Disposables;
using Tauron.Application.Blazor.UI;
using Tauron.Application.CommonUI.UI;

namespace Tauron.Application.Blazor
{
    public sealed class ActiveBinding<TData> : IInternalbinding
    {
        private readonly TData _defaultValue;

        private readonly object _lock = new();
        private IDisposable? _disposer;

        private TData? _lastData;
        private string? _lastError;

        private DeferredSource? _source;
        private IDisposable? _subscription;
        private Action? _updateState;

        internal ActiveBinding(TData defaultValue) => _defaultValue = defaultValue;

        public static ActiveBinding<TData> Empty => new(default!);

        public TData Value
        {
            get
            {
                if (_source?.Value is TData data)
                    return data;

                return _defaultValue;
            }
            set
            {
                if (_source != null)
                    _source.Value = value;
            }
        }

        public bool HasErrors => _source?.HasErrors ?? false;

        public string? Error => _source?.Error;

        void IInternalbinding.Update(DeferredSource source, Action updateState)
        {
            Dispose();

            _source = source;
            _updateState = updateState;

            source.PropertyChanged += SourceOnPropertyChanged;
            source.ErrorsChanged += ErrorSourceOnErrorsChanged;
            _disposer = Disposable.Create(
                () =>
                {
                    source.PropertyChanged -= SourceOnPropertyChanged;
                    source.ErrorsChanged -= ErrorSourceOnErrorsChanged;
                });
            UpdateSubscription();
        }

        public void Dispose()
        {
            _disposer?.Dispose();
            _subscription?.Dispose();
            _source = null;
            _updateState = null!;
        }

        private void ErrorSourceOnErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
        {
            if (string.Equals(_lastError, Error, StringComparison.Ordinal)) return;

            _lastError = Error;
            _updateState?.Invoke();
        }

        private void SourceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Value") return;

            if (_lastData?.Equals(Value) == true) return;

            _lastData = Value;

            UpdateSubscription();
            _updateState?.Invoke();
        }


        private void UpdateSubscription()
        {
            lock (_lock)
            {
                var currentValue = Value;

                switch (currentValue)
                {
                    case INotifyCollectionChanged collectionChanged:
                        collectionChanged.CollectionChanged += ValueOnCollectionChanged;
                        _subscription = Disposable.Create(() => collectionChanged.CollectionChanged -= ValueOnCollectionChanged);

                        break;
                    case INotifyPropertyChanged changed:
                        changed.PropertyChanged += ValueOnPropertyChanged;
                        _subscription = Disposable.Create(() => changed.PropertyChanged -= ValueOnPropertyChanged);

                        break;
                }
            }
        }

        private void ValueOnPropertyChanged(object? sender, PropertyChangedEventArgs e) => _updateState?.Invoke();

        private void ValueOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => _updateState?.Invoke();
    }
}