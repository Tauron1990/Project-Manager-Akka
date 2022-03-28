using System.Reactive;
using System.Reactive.Disposables;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Services;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Shared.ViewModels.CurrentJobs;

public sealed class JobDetailDisplayViewModel : ViewModelBase
{
    public ReactiveCommand<ProjectId?, Unit> EditJob { get; }

    private ObservableAsPropertyHelper<JobData?>? _jobData;
    public JobData? JobData => _jobData?.Value;
    
    public JobDetailDisplayViewModel(GlobalState globalState, PageNavigation navigationManager, IMessageMapper aggregator) 
    {
        this.WhenActivated(dispo => _jobData = globalState.Jobs
                              .CurrentlySelectedData
                              .ToProperty(this, model => model.JobData)
                              .DisposeWith(dispo));
        
        EditJob = ReactiveCommand.Create<ProjectId?, Unit>(
                    id =>
                    {
                        if (id is null)
                        {
                            aggregator.PublishWarnig("Keine Projekt id Verfügbar");

                            return Unit.Default;
                        }

                        navigationManager.EditJob(id);

                        return Unit.Default;
                    }, globalState.IsOnline).DisposeWith(Disposer);
    }
}