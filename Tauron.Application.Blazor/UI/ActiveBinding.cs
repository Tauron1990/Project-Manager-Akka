using System;
using Tauron.Application.CommonUI.UI;

namespace Tauron.Application.Blazor.UI
{
    public sealed class ActiveBinding<TData> : IDisposable
    {
        private DeferredSource? _source;
        private IDisposable? _disposer;
        private Action _updateState;

        public TData? Value
        {
            get
            {
                if(_)
            }
            set;
        }

        internal void Update(DeferredSource source, IDisposable disposer, Action updateState)
        {

        }

        public void Dispose()
        {
            _disposer?.Dispose();
            _source = null;
            _updateState = null;
        }
    }
}