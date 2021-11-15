using System;
using System.Reactive.Disposables;
using System.Windows.Input;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;

namespace Tauron.Application.Blazor;

public sealed record CommandState(Action Execute, bool Disabled)
{
    public static readonly CommandState Default = new(() => {}, Disabled: true);
}

public partial class CommandHelper
{
    private object? _commandParameter;
    private ICommand? _command;

    [UsedImplicitly]
    public ICommand? Command
    {
        get => _command;
        set
        {
            _command = value;
            InvokeAsync(StateHasChanged);
        }
    }

    [UsedImplicitly]
    public object? CommandParameter
    {
        get => _commandParameter;
        set
        {
            _commandParameter = value;
            InvokeAsync(StateHasChanged);
        }
    }
    
    [Parameter]
    public RenderFragment<CommandState>? ChildContent { get; set; }

    private CommandState _commandState = CommandState.Default;
    
    private SerialDisposable _subscription = new();

    protected override void OnInitialized()
    {
        _subscription.DisposeWith(this);
        base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        if (Command is null)
        {
            _commandState = CommandState.Default;
            _subscription.Disposable = Disposable.Empty;
        }
        else
        {
            Command.CanExecuteChanged += CanExecuteChanged;
            _subscription.Disposable = Disposable.Create(
                (Command, SelfReference:this), 
                data => data.Command.CanExecuteChanged += data.SelfReference.CanExecuteChanged);
            
            CreateCommandState();
        }
        base.OnParametersSet();
    }

    private void CanExecuteChanged(object? sender, EventArgs e)
    {
        CreateCommandState();
        InvokeAsync(StateHasChanged);
    }

    private void CreateCommandState()
        => _commandState = new CommandState(RunCommand, !Command?.CanExecute(CommandParameter) ?? true);

    private void RunCommand()
        => Command?.Execute(CommandParameter);
}