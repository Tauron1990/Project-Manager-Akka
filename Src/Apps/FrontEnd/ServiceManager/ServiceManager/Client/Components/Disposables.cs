using System;
using System.Threading;

namespace ServiceManager.Client.Components
{
    public static class Disposables
    {
        private sealed class ActionDispose : IDisposable
        {
            private Action? _action;

            public ActionDispose(Action action) => _action = action;

            public void Dispose() => Interlocked.Exchange(ref _action, null)?.Invoke();
        }

        public static IDisposable Create(Action action)
            => new ActionDispose(action);
    }
}