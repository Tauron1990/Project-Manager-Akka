using System;
using System.Reactive.Disposables;
using System.Windows.Input;
using Microsoft.AspNetCore.Components;

namespace Tauron.Application.Blazor.Commands;

public partial class CommandHelper
{
    private CommandState _commandState = CommandState.Default;

    private SerialDisposable _subscription = new();

    [CascadingParameter(Name = nameof(Command))]
    public ICommand? Command { get; set; }

    [CascadingParameter(Name = nameof(CommandParameter))]
    public object? CommandParameter { get; set; }

    [Parameter]
    public RenderFragment<CommandState>? ChildContent { get; set; }

    protected override void OnInitialized()
    {
        _subscription.DisposeWith(this);
        base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        if(Command is null)
        {
            _commandState = CommandState.Default;
            _subscription.Disposable = Disposable.Empty;
        }
        else
        {
            Command.CanExecuteChanged += CanExecuteChanged;
            _subscription.Disposable = Disposable.Create(
                (Command, SelfReference: this),
                data => data.Command.CanExecuteChanged += data.SelfReference.CanExecuteChanged);

            CreateCommandState();
        }

        base.OnParametersSet();
    }

    private void CanExecuteChanged(object? sender, EventArgs e)
    {
        CreateCommandState();
        InvokeAsync(StateHasChanged).Ignore();
    }

    private void CreateCommandState()
        => _commandState = new CommandState(RunCommand, !Command?.CanExecute(CommandParameter) ?? true);

    private void RunCommand()
        => Command?.Execute(CommandParameter);
}