using ReactiveUI;
using Tauron.Application.Blazor.Commands;

namespace SimpleProjectManager.Client.Pages;

public partial class Dashboard
{
    private MudCommandButton? _startJob;

    public MudCommandButton? StartJob
    {
        get => _startJob;
        set => this.RaiseAndSetIfChanged(ref _startJob, value);
    }

    protected override IEnumerable<IDisposable> InitializeModel()
    {
        yield return this.BindCommand(ViewModel, m => m.StartJob, d => d.StartJob);
    }
}