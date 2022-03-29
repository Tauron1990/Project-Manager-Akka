using System.Reactive.Disposables;
using Microsoft.AspNetCore.Components;
using ReactiveUI;
using SimpleProjectManager.Shared.Services;
using Tauron.Application.Blazor.Commands;

namespace SimpleProjectManager.Client.Shared.CurrentJobs;

public partial class JobSideBar
{
    private MudCommandButton? _newJob;

    public MudCommandButton? NewJob
    {
        get => _newJob;
        set => this.RaiseAndSetIfChanged(ref _newJob, value);
    }

    protected override IEnumerable<IDisposable> InitializeModel()
    {
        yield return this.BindCommand(ViewModel, m => m.NewJob, v => v.NewJob);
    }
}