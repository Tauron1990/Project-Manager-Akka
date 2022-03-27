using Microsoft.AspNetCore.Components;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data.JobEdit;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Pages;

public partial class EditJob
{
    [Parameter]
    public string ProjectId { get; set; } = string.Empty;

    private Action<JobEditorCommit> CommitAction = _ => { };

    protected override void InitializeModel()
    {
        this.WhenActivated(
            _ =>
            {
                if (ViewModel == null) return;

                CommitAction = ViewModel.Commit.ToAction();
            });
        base.InitializeModel();
    }
}