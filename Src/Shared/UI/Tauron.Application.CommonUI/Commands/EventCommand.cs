using System;
using JetBrains.Annotations;

namespace Tauron.Application.CommonUI.Commands;

[PublicAPI]
public class EventCommand : CommandBase
{
    #pragma warning disable MA0046
    public event Func<object?, bool>? CanExecuteEvent;

    public event Action<object?>? ExecuteEvent;
    #pragma warning restore MA0046

    public sealed override bool CanExecute(object? parameter) => OnCanExecute(parameter);

    public sealed override void Execute(object? parameter)
    {
        OnExecute(parameter);
    }

    protected virtual bool OnCanExecute(object? parameter)
    {
        var handler = CanExecuteEvent;

        return handler is null || handler(parameter);
    }

    protected virtual void OnExecute(object? parameter)
    {
        ExecuteEvent?.Invoke(parameter);
    }

    public void Clear()
    {
        CanExecuteEvent = null;
        ExecuteEvent = null;
    }
}