using System.Reactive.Disposables;
using ReactiveUI;
using SimpleProjectManager.Client.ViewModels;
using Tauron.Application.Blazor.Commands;

namespace SimpleProjectManager.Client.Pages;

public partial class CurrentJobs
{
    private MudCommandButton? _newJob;

    private MudCommandButton? NewJob
    
    {
        get => _newJob;
        set => this.RaiseAndSetIfChanged(ref _newJob, value);
    }

    protected override CurrentJobsViewModel CreateModel()
        => Services.GetRequiredService<CurrentJobsViewModel>();

    protected override void InitializeModel()
    {
        this.WhenActivated(
            dispo =>
            {
                this.BindCommand(ViewModel, m => m.NewJob, v => v.NewJob)
                   .DisposeWith(dispo);
            });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _model.Reset();
    }
}