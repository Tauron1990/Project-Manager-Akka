using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Host;

namespace Tauron.Application.CommonUI
{
    //[DebuggerNonUserCode]
    [PublicAPI]
    [Serializable]
    public class UIObservableCollection<TType> : ObservableCollection<TType>
    {
        private bool _isBlocked;

        public UIObservableCollection()
        {
        }

        public UIObservableCollection(IEnumerable<TType> enumerable)
            : base(enumerable)
        {
        }

        [NotNull]
        protected IUIDispatcher InternalUISynchronize { get; } =
            ActorApplication.ServiceProvider.GetRequiredService<IUIDispatcher>();

        public void AddRange(IEnumerable<TType> enumerable)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
            foreach (var item in enumerable) Add(item);
        }

        public IDisposable BlockChangedMessages() => new DispoableBlocker(this);

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_isBlocked) return;
            if (InternalUISynchronize.CheckAccess())
                base.OnCollectionChanged(e);
            InternalUISynchronize.Post(() => base.OnCollectionChanged(e));
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (_isBlocked) return;
            if (InternalUISynchronize.CheckAccess()) base.OnPropertyChanged(e);
            else InternalUISynchronize.Post(() => base.OnPropertyChanged(e));
        }

        private class DispoableBlocker : IDisposable
        {
            private readonly UIObservableCollection<TType> _collection;

            public DispoableBlocker(UIObservableCollection<TType> collection)
            {
                _collection = collection;
                _collection._isBlocked = true;
            }

            public void Dispose()
            {
                _collection._isBlocked = false;
                _collection.OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
    }
}