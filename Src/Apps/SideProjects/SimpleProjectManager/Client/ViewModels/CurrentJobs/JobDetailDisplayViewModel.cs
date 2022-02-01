using System.Reactive;
using ReactiveUI;
using SimpleProjectManager.Client.Data;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public class JobDetailDisplayViewModel : BlazorViewModel
{
    public ReactiveCommand<ProjectId?, Unit> EditJob { get; }

    private readonly ObservableAsPropertyHelper<JobData?> _jobData;
    public JobData? JobData => _jobData.Value;
    
    public JobDetailDisplayViewModel(IStateFactory stateFactory, GlobalState globalState, PageNavigation navigationManager, IEventAggregator aggregator) 
        : base(stateFactory)
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
            }, globalState.IsOnline);
    }
}