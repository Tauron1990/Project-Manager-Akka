using System.Reactive.Linq;
using SimpleProjectManager.Shared.Services;
using SimpleProjectManager.Shared.Validators;
using Stl.Fusion;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Application;

namespace SimpleProjectManager.Client.Data.States;

public sealed partial class JobsState : StateBase<InternalJobData>
{
    private readonly UpdateProjectCommandValidator _upodateProjectValidator = new();
    private readonly ProjectNameValidator _nameValidator = new();
    
    private readonly IJobDatabaseService _service;
    private readonly IEventAggregator _eventAggregator;
    
    public JobsState(IStoreConfiguration storeConfiguration, IStateFactory stateFactory, IJobDatabaseService jobDatabaseService, IEventAggregator eventAggregator)
        : base(storeConfiguration, stateFactory)
    {
        _service = jobDatabaseService;
        _eventAggregator = eventAggregator;
    }

    protected override IStateConfiguration<InternalJobData> ConfigurateState(ISourceConfiguration<InternalJobData> configuration)
    {
        return ConfigurateEditor
            (
                configuration.FromCacheAndServer<(JobInfo[], SortOrder[])>(
                    async token =>
                    {
                        var jobs = await _service.GetActiveJobs(token);
                        var order = await _service.GetSortOrders(token);

                        return (jobs, order);
                    },
                    (originalData, serverData) =>
                    {
                        var (jobs, orders) = serverData;
                        var pairs = jobs.Select(j => new JobSortOrderPair(orders.First(o => o.Id == j.Project), j)).ToArray();

                        var selection = originalData.CurrentSelected;
                        // ReSharper disable once AccessToModifiedClosure
                        if (selection.Pair != null && pairs.All(s => s.Info.Project != selection.Pair.Info.Project))
                            selection = new CurrentSelected(null, null);

                        return originalData with { CurrentJobs = pairs, CurrentSelected = selection };
                    })
            ).ApplyReducers(
                f => f.On<SelectNewPairAction>(JobDataPatcher.ReplaceSlected))
           .ApplyRequests(
                requestFactory =>
                    requestFactory.OnTheFlyUpdate(
                            JobDataSelectors.CurrentSelected,
                            (cancel, source) => JobDataRequests.FetchjobData(source, _service, cancel),
                            JobDataPatcher.ReplaceSelected)
                       .AddRequest<SetSortOrder>(_service.ChangeOrder!, JobDataPatcher.PatchSortOrder));
    }

    protected override void PostConfiguration(IRootStoreState<InternalJobData> state)
    {
        CurrentlySelectedData = state.Select(jobData => jobData.CurrentSelected.JobData);
        CurrentlySelectedPair = state.Select(jobData => jobData.CurrentSelected.Pair);
        CurrentJobs = state.Select(jobData => jobData.CurrentJobs);
        ActiveJobsCount = FromServer(_service.CountActiveJobs);
    }
    
    public IObservable<JobSortOrderPair[]> CurrentJobs { get; private set; } = Observable.Empty<JobSortOrderPair[]>();

    public IObservable<JobData?> CurrentlySelectedData { get; private set; } = Observable.Empty<JobData>();

    public IObservable<JobSortOrderPair?> CurrentlySelectedPair { get; private set; } = Observable.Empty<JobSortOrderPair>();

    public IObservable<long> ActiveJobsCount { get; private set; } = Observable.Empty<long>();
    
}