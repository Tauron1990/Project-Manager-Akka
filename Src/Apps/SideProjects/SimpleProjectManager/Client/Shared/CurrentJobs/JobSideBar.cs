using System.Reactive.Disposables;
using ReactiveUI;
using SimpleProjectManager.Client.ViewModels;
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

    protected override JobSidebarModel CreateModel()
        => Services.GetRequiredService<JobSidebarModel>();

    protected override void InitializeModel()
    {
        this.WhenActivated(
            dispo =>
            {
                this.BindCommand(ViewModel, m => m.NewJob, v => v.NewJob)
                   .DisposeWith(dispo);
            });
    }
}