using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron.Application.CommonUI.Model
{
    [PublicAPI]
    public sealed class CommandRegistrationBuilder
    {
        private readonly List<IObservable<bool>> _canExecute = new();
        private readonly Func<string, Action<object?>, IObservable<bool>?, UIPropertyBase> _register;

        private Delegate? _command;

        internal CommandRegistrationBuilder(Func<string, Action<object?>, IObservable<bool>?, UIPropertyBase> register,
            IObservableActor target)
        {
            Target = target;
            _register = register;
        }

        public IObservableActor Target { get; }

        public CommandRegistrationBuilder WithExecute(Action<object?> execute,
            IEnumerable<IObservable<bool>>? canExecute)
        {
            _command = Delegate.Combine(_command, execute);

            return canExecute != null ? WithCanExecute(canExecute) : this;
        }

        public CommandRegistrationBuilder WithExecute(Action execute, IEnumerable<IObservable<bool>> canExecute)
        {
            _command = Delegate.Combine(_command, new ActionMapper(execute).Action);

            return WithCanExecute(canExecute);
        }

        public CommandRegistrationBuilder WithExecute(Action<object?> execute, IObservable<bool>? canExecute)
        {
            _command = Delegate.Combine(_command, execute);

            return canExecute != null ? WithCanExecute(canExecute) : this;
        }

        public CommandRegistrationBuilder WithExecute(Action<object?> execute)
        {
            _command = Delegate.Combine(_command, execute);

            return this;
        }

        public CommandRegistrationBuilder WithExecute(Action execute, IObservable<bool> canExecute)
        {
            _command = Delegate.Combine(_command, new ActionMapper(execute).Action);

            return WithCanExecute(canExecute);
        }

        public CommandRegistrationBuilder WithExecute(Action execute)
        {
            _command = Delegate.Combine(_command, new ActionMapper(execute).Action);
            return this;
        }

        public CommandRegistrationBuilder WithCanExecute(IEnumerable<IObservable<bool>> canExecute)
        {
            _canExecute.AddRange(canExecute);

            return this;
        }

        public CommandRegistrationBuilder WithCanExecute(IObservable<bool> canExecute)
        {
            _canExecute.Add(canExecute);

            return this;
        }

        public UIPropertyBase? ThenRegister(string name)
        {
            if (_command == null) return null;

            var canExec = _canExecute.Count switch
            {
                0 => null,
                1 => _canExecute[0],
                _ => _canExecute.CombineLatest(list => list.All(b => b))
            };

            return _register(name, (Action<object?>) _command, canExec);
        }

        private sealed class ActionMapper
        {
            private readonly Action _action;

            public ActionMapper(Action action) => _action = action;

            public Action<object?> Action => ActionImpl;

            private void ActionImpl(object? o)
            {
                _action();
            }
        }

        //private sealed class FuncMapper
        //{
        //    private readonly Func<bool> _action;

        //    public FuncMapper(Func<bool> action)
        //    {
        //        _action = action;
        //    }

        //    public Func<object?, bool> Action => ActionImpl;

        //    private bool ActionImpl(object? o)
        //    {
        //        return _action();
        //    }
        //}
    }
}