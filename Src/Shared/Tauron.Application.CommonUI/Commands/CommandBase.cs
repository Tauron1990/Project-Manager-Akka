using System;
using System.Windows.Input;
using JetBrains.Annotations;

namespace Tauron.Application.CommonUI.Commands
{
    [PublicAPI]
    public abstract class CommandBase : ICommand
    {
        public static readonly ICommand Empty = new SimpleCommand(() => { });

        private readonly WeakReferenceCollection<WeakDelegate> _referenceCollection = new();

        public event EventHandler? CanExecuteChanged
        {
            add
            {
                if (value != null)
                    _referenceCollection.Add(new WeakDelegate(value));
            }
            remove
            {
                if (value != null)
                    _referenceCollection.Remove(new WeakDelegate(value));
            }
        }

        public virtual bool CanExecute(object? parameter = null) => true;

        public abstract void Execute(object? parameter = null);

        public virtual void RaiseCanExecuteChanged()
        {
            foreach (var weakDelegate in _referenceCollection)
                weakDelegate.Invoke(this, EventArgs.Empty);
        }
    }
}