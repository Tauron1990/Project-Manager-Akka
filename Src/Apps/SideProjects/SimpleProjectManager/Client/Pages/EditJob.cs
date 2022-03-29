using Microsoft.AspNetCore.Components;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data.JobEdit;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Pages;

public partial class EditJob
{
    [Parameter]
    public string ProjectId { get; set; } = string.Empty;

    private Action<JobEditorCommit> _commitAction = _ => { };

    protected override IEnumerable<IDisposable> InitializeModel()
    {

        if (ViewModel == null) yield break;

        _commitAction = ViewModel.Commit.ToAction();
    }
}