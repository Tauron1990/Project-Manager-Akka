using Microsoft.AspNetCore.Components;
using SimpleProjectManager.Client.Shared.Data.JobEdit;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Pages;

public partial class EditJob
{
    private Action<JobEditorCommit> _commitAction = _ => { };

    [Parameter]
    public string ProjectId { get; set; } = string.Empty;

    protected override IEnumerable<IDisposable> InitializeModel()
    {

        if(ViewModel == null) yield break;

        ViewModel.JobId.Set(ProjectId);
        _commitAction = ViewModel.Commit.ToAction();
    }

    protected override void OnParametersSet()
    {
        ViewModel?.JobId.Set(ProjectId);
        base.OnParametersSet();
    }
}