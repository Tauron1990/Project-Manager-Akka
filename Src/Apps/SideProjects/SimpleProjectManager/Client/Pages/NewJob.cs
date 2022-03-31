using SimpleProjectManager.Client.Shared.Data.JobEdit;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Pages;

public partial class NewJob
{
    private Action? _cancel;

    private Action<JobEditorCommit>? _commit;

    protected override IEnumerable<IDisposable> InitializeModel()
    {
        if (ViewModel == null) yield break;

        _cancel = ViewModel.Cancel?.ToAction();
        _commit = ViewModel.Commit?.ToAction();
    }
}