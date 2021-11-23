using System.Reactive.Disposables;
using Microsoft.AspNetCore.Components;
using ReactiveUI;
using SimpleProjectManager.Client.ViewModels;
using SimpleProjectManager.Shared.Services;
using Tauron.Application.Blazor.Commands;

namespace SimpleProjectManager.Client.Shared.CurrentJobs;

public partial class JobSideBar
{
    [Parameter]
    public JobInfo[]? CurrentJobs { get; set; }
    
    private MudCommandButton? _newJob;

    public MudCommandButton? NewJob
    {
        get => _newJob;
        set => this.RaiseAndSetIfChanged(ref _newJob, value);
    }

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