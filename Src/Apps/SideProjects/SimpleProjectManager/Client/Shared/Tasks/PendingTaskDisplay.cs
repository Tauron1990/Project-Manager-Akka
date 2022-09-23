using System.Reactive.Disposables;
using Microsoft.AspNetCore.Components;
using ReactiveUI;
using SimpleProjectManager.Shared.Services.Tasks;
using Tauron.Application.Blazor.Commands;

namespace SimpleProjectManager.Client.Shared.Tasks;

public partial class PendingTaskDisplay
{
    private MudCommandButton? _cancelButton;

    [Parameter]
    public PendingTask? PendingTask { get; set; }

    public MudCommandButton? CancelButton
    {
        get => _cancelButton;
        set => this.RaiseAndSetIfChanged(ref _cancelButton, value);
    }

    protected override IEnumerable<IDisposable> InitializeModel()
    {
        ViewModel?.PendingTask.Set(PendingTask);
        yield return this.BindCommand(ViewModel, m => m.Cancel, d => d.CancelButton);
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        ViewModel?.PendingTask.Set(PendingTask);
    }
}