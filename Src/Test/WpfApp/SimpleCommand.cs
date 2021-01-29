using System;
using System.Windows.Input;

namespace WpfApp
{
    public class SimpleCommand : ICommand
    {
        private readonly Action _action;

        public SimpleCommand(Action action) => _action = action;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _action();

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}