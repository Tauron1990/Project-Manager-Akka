using System;

namespace Tauron.Application.CommonUI.Helper
{
    public abstract class ControlBindableBase : IControlBindable
    {
        protected IUIObject Root { get; private set; } = new Dummy();

        protected IUIObject AffectedObject { get; private set; } = new Dummy();

        public IDisposable Bind(IUIObject root, IUIObject affectedObject, object dataContext)
        {
            Root = root;
            AffectedObject = affectedObject;
            Bind(dataContext);

            return new CleanUpHelper(this);
        }

        public IDisposable NewContext(object newContext)
        {
            Bind(newContext);
            return new CleanUpHelper(this);
        }

        protected abstract void CleanUp();

        protected abstract void Bind(object context);

        private class CleanUpHelper : IDisposable
        {
            private readonly ControlBindableBase _control;
            private bool _isDisposed;

            public CleanUpHelper(ControlBindableBase control) => _control = control;

            public void Dispose()
            {
                if (_isDisposed) return;

                _control.CleanUp();
                _isDisposed = true;
            }
        }

        private class Dummy : IUIObject
        {
            public IUIObject? GetPerent() => null;

            public object Object => new();
        }
    }
}