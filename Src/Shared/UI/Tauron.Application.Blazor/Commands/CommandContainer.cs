using System.Windows.Input;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;

namespace Tauron.Application.Blazor.Commands;

public class CommandContainer : ComponentBase
{
    private ICommand? _command;
    private object? _commandParameter;

    [UsedImplicitly]
    public ICommand? Command
    {
        get => _command;
        set
        {
            _command = value;
            InvokeAsync(StateHasChanged).Ignore();
        }
    }

    [UsedImplicitly]
    public object? CommandParameter
    {
        get => _commandParameter;
        set
        {
            _commandParameter = value;
            InvokeAsync(StateHasChanged).Ignore();
        }
    }
}