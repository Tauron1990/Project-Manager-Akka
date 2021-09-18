using System;
using JetBrains.Annotations;

namespace Tauron.Application.CommonUI.Commands
{
    [PublicAPI]
    public class SimpleCommand : CommandBase
    {
        private readonly Func<object?, bool>? _canExecute;

        private readonly Action<object?> _execute;
        private readonly object? _parameter;

        public SimpleCommand(Func<object?, bool>? canExecute, Action<object?> execute, object? parameter = null)
        {
            _canExecute = canExecute;
            _execute = execute;
            _parameter = parameter;
        }

        public SimpleCommand(Action<object?> execute)
            : this(null, execute)
        {
        }

        public SimpleCommand(Func<bool>? canExecute, Action execute)
        {
            _execute = _ => execute();
            if (canExecute is not null)
                _canExecute = _ => canExecute();
        }

        public SimpleCommand(Action execute)
            : this(null, execute)
        {
        }

        public override bool CanExecute(object? parameter = null)
        {
            parameter ??= _parameter;

            return _canExecute is null || _canExecute(parameter);
        }

        public override void Execute(object? parameter = null)
        {
            parameter ??= _parameter;
            _execute(parameter);
        }
    }
}