using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Services;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Shared.ViewModels.CurrentJobs;

public sealed class JobDetailDisplayViewModel : ViewModelBase
{
    public ReactiveCommand<ProjectId?, Unit>? EditJob { get; private set; }

    private ObservableAsPropertyHelper<JobData?>? _jobData;
    public JobData? JobData => _jobData?.Value;
    
    public JobDetailDisplayViewModel(GlobalState globalState, PageNavigation navigationManager, IMessageDispatcher aggregator) 
    {
        Console.WriteLine("JobState Detail View Constructor");
        
        this.WhenActivated(Init);
        
        IEnumerable<IDisposable> Init()
        {
            Console.WriteLine("JobState Detail View Init");
            
            yield return _jobData = globalState.Jobs
               .CurrentlySelectedData
               .ObserveOn(RxApp.MainThreadScheduler)
               .ToProperty(this, model => model.JobData);
            
            yield return EditJob = ReactiveCommand.Create<ProjectId?, Unit>(
                id =>
                {
                    if (id is null)
                    {
                        aggregator.PublishWarnig("Keine Projekt id Verfügbar");

                        return Unit.Default;
                    }

                    navigationManager.EditJob(id);

                    return Unit.Default;
                },
                globalState.IsOnline);
        }
    }
}