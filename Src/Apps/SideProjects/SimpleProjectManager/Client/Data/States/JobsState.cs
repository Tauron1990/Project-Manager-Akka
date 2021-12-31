using SimpleProjectManager.Client.Data.Core;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Data.States;

public sealed class JobsState
{
    private sealed record CurrentSelected(ProjectId? Id, JobData? JobData);
    
    private sealed record InternalJobData(JobSortOrderPair[] CurrentJobs, CurrentSelected CurrentSelected)
    {
        public InternalJobData()
            : this(Array.Empty<JobSortOrderPair>(), new CurrentSelected(null, null))
        { }
    }
    
    public JobsState(IStoreConfiguration storeConfiguration, IJobDatabaseService service)
    {
        storeConfiguration.NewState<InternalJobData>(
            c => c.FromCacheAndServer<(JobInfo[], SortOrder[])>(
                    async token =>
                    {
                        var jobs = await service.GetActiveJobs(token);
                        var order = await service.GetSortOrders(token);

                        return (jobs, order);
                    },
                    (originalData, serverData) =>
                    {
                        var (jobs, orders) = serverData;
                        var pairs = jobs.Select(j => new JobSortOrderPair(orders.First(o => o.Id == j.Project), j));

                        return originalData with { CurrentJobs = pairs.ToArray() };
                    })
               .ApplyRequests(
                    requestFactory =>
                        requestFactory.OnTheFlyUpdate(
                            jobData => jobData.CurrentSelected,
                            async (cancel, source) =>
                            {
                                var id = (await source(cancel)).Id;

                                if (id is null)
                                    return new CurrentSelected(null, null);

                                var jobData = await service.GetJobData(id, cancel);

                                return new CurrentSelected(id, jobData);
                            },
                            (state, data) => state with { CurrentSelected = data }))
               .AndFinish());
    }
}