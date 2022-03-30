using System.Reactive.Disposables;
using Microsoft.AspNetCore.Components;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data.JobEdit;
using SimpleProjectManager.Client.Shared.ViewModels;
using SimpleProjectManager.Client.Shared.ViewModels.EditJob;
using SimpleProjectManager.Client.ViewModels;
using Tauron.Application.Blazor.Commands;
using Tauron;

namespace SimpleProjectManager.Client.Shared.EditJob;

public partial class JobEditor
{
    private MudCommandButton? _cancelButton;
    private MudCommandButton? _commitButton;

    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<JobEditorCommit> Commit { get; set; }

    [Parameter]
    public EventCallback Cancel { get; set; }

    [Parameter]
    public bool CanCancel { get; set; }
    
    [Parameter]
    public JobEditorConfiguration Configuration { get; set; } = JobEditorConfiguration.Default;

    [Parameter]
    public string Style { get; set; } = string.Empty;

    private bool StatusEditing => Configuration.StatusEditing;

    private bool SortOrderEditing => Configuration.SortOrderEditing;

    [Parameter]
    public JobEditorData? Data { get; set; }

    [Parameter]
    public JobEditorViewModel? CustomViewModel { get; set; }

    private MudCommandButton? CancelButton
    {
        get => _cancelButton;
        set => this.RaiseAndSetIfChanged(ref _cancelButton, value);
    }

    private MudCommandButton? CommitButton
    {
        get => _commitButton;
        set => this.RaiseAndSetIfChanged(ref _commitButton, value);
    }

    protected override JobEditorViewModel CreateModel()
        => CustomViewModel ?? base.CreateModel();

    protected override IEnumerable<IDisposable> InitializeModel()
    {
        if (ViewModel is null) yield break;

        yield return ViewModel.Data?.Changed.Subscribe(_ => RenderingManager.StateHasChangedAsync().Ignore()) ?? Disposable.Empty;

        yield return this.BindCommand(ViewModel, m => m.Cancel, v => v.CancelButton);
        yield return this.BindCommand(ViewModel, m => m.Commit, v => v.CommitButton);
    }
}