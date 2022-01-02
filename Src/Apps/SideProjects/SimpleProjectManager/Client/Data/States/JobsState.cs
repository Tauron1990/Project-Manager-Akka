using System.Reactive.Linq;
using SimpleProjectManager.Client.Data.Core;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Data.States;

public sealed class JobsState : StateBase<InternalJobData>
{
    private readonly IJobDatabaseService _service;
    
    public JobsState(IStoreConfiguration storeConfiguration, IServiceProvider serviceProvider)
        : base(storeConfiguration)
        => _service = serviceProvider.GetRequiredService<IJobDatabaseService>();

    protected override IStateConfiguration<InternalJobData> ConfigurateState(ISourceConfiguration<InternalJobData> configuration)
        => configuration.FromCacheAndServer<(JobInfo[], SortOrder[])>(
                async token =>
                {
                    var jobs = await _service.GetActiveJobs(token);
                    var order = await _service.GetSortOrders(token);

                    return (jobs, order);
                },
                (originalData, serverData) =>
                {
                    var (jobs, orders) = serverData;
                    var pairs = jobs.Select(j => new JobSortOrderPair(orders.First(o => o.Id == j.Project), j));

                    return originalData with { CurrentJobs = pairs.ToArray() };
                })
           .ApplyReducers(
                f => f.On<SelectNewPairAction>(InternalJobDataPatcher.ReplaceSlected))
           .ApplyRequests(
                requestFactory =>
                    requestFactory.OnTheFlyUpdate(
                        IntenalJobDataSelectors.CurrentSelected,
                        (cancel, source) => InternalDataRequests.FetchjobData(cancel, source, _service),
                        InternalJobDataPatcher.ReplaceSelected)
                       .AddRequest<SetSortOrder>(_service.ChangeOrder, InternalJobDataPatcher.PatchSortOrder));

    protected override void PostConfiguration(IRootStoreState<InternalJobData> state)
    {
        CurrentlySelectedData = state.Select(jobData => jobData.CurrentSelected.JobData);
        CurrentlySelectedPair = state.Select(jobData => jobData.CurrentSelected.Pair);
        CurrentJobs = state.Select(jobData => jobData.CurrentJobs);
    }
    
    public IObservable<JobSortOrderPair[]> CurrentJobs { get; private set; } = Observable.Empty<JobSortOrderPair[]>();

    public IObservable<JobData?> CurrentlySelectedData { get; private set; } = Observable.Empty<JobData>();

    public IObservable<JobSortOrderPair?> CurrentlySelectedPair { get; private set; } = Observable.Empty<JobSortOrderPair>();
    
    public void NewSelection(JobSortOrderPair? newSelection)
        => Dispatch(new SelectNewPairAction(newSelection));

    public void SetNewSortOrder(SetSortOrder order)
        => Dispatch(order);
}