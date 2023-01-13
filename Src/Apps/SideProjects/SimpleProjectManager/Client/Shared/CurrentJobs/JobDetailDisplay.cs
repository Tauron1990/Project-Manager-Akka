using ReactiveUI;
using Tauron.Application.Blazor.Commands;

namespace SimpleProjectManager.Client.Shared.CurrentJobs;

public partial class JobDetailDisplay
{
    private MudCommandButton? _editButton;

    private MudCommandButton? EditButton
    {
        get => _editButton;
        set => this.RaiseAndSetIfChanged(ref _editButton, value);
    }

    protected override IEnumerable<IDisposable> InitializeModel()
    {
        if(ViewModel == null) yield break;

        yield return this.BindCommand(
            ViewModel,
            m => m.EditJob,
            v => v.EditButton,
            ViewModel.WhenAny(m => m.JobData, m => m.Value?.Id));
    }
}