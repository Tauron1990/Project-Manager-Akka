using System.Reactive.Disposables;
using Microsoft.AspNetCore.Components;
using ReactiveUI;
using SimpleProjectManager.Client.ViewModels;
using Tauron.Application.Blazor;
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

    private bool StatusEditing => Configuration.StatusEditing;

    private bool SortOrderEditing => Configuration.SortOrderEditing;

    [Parameter]
    public JobEditorData? Data { get; set; }

    protected override JobEditorViewModel CreateModel()
        => Services.GetIsolatedService<JobEditorViewModel>().DisposeWith(this);

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

    protected override void InitializeModel()
    {
        this.WhenActivated(
            dispo =>
            {
                this.BindCommand(ViewModel, m => m.Cancel, v => v.CancelButton).DisposeWith(dispo);
                this.BindCommand(ViewModel, m => m.Commit, v => v.CommitButton).DisposeWith(dispo);
            });
    }
}