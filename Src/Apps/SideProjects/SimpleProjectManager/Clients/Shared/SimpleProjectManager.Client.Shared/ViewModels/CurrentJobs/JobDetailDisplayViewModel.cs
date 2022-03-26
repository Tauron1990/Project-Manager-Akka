using System.Reactive;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Services;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Tauron;

namespace SimpleProjectManager.Client.Shared.ViewModels.CurrentJobs;

public sealed class JobDetailDisplayViewModel : ViewModelBase
{
    public ReactiveCommand<ProjectId?, Unit> EditJob { get; }

    private readonly ObservableAsPropertyHelper<JobData?> _jobData;
    public JobData? JobData => _jobData.Value;
    
    public JobDetailDisplayViewModel(GlobalState globalState, PageNavigation navigationManager, IMessageMapper aggregator) 
    {
        _jobData = globalState.Jobs.CurrentlySelectedData.ToProperty(this, model => model.JobData);
        
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
            }, globalState.IsOnline)
           .DisposeWith(this);
    }
}