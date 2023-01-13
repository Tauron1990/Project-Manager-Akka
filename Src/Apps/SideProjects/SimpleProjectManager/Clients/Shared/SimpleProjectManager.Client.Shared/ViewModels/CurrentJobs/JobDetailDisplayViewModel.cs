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
    private readonly IMessageDispatcher _aggregator;
    private readonly GlobalState _globalState;
    private readonly PageNavigation _navigationManager;
    private ObservableAsPropertyHelper<JobData?>? _jobData;

    public JobDetailDisplayViewModel(GlobalState globalState, PageNavigation navigationManager, IMessageDispatcher aggregator)
    {
        _globalState = globalState;
        _navigationManager = navigationManager;
        _aggregator = aggregator;
        #if DEBUG
        Console.WriteLine("JobState Detail View Constructor");
        #endif

        this.WhenActivated(Init);


    }

    public ReactiveCommand<ProjectId?, Unit>? EditJob { get; private set; }
    public JobData? JobData => _jobData?.Value;

    private IEnumerable<IDisposable> Init()
    {
        #if DEBUG
        Console.WriteLine("JobState Detail View Init");
        #endif

        yield return _jobData = _globalState.Jobs
           .CurrentlySelectedData
           .ObserveOn(RxApp.MainThreadScheduler)
           .ToProperty(this, model => model.JobData);

        yield return EditJob = ReactiveCommand.Create<ProjectId?, Unit>(
            id =>
            {
                if(id is null)
                {
                    _aggregator.PublishWarnig("Keine Projekt id Verfügbar");

                    return Unit.Default;
                }

                _navigationManager.EditJob(id);

                return Unit.Default;
            },
            _globalState.IsOnline);
    }
}