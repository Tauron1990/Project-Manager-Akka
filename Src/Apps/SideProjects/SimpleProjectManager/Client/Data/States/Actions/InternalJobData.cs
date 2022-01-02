using SimpleProjectManager.Shared.Services;
using Tauron;

namespace SimpleProjectManager.Client.Data.States;

internal static class InternalJobDataPatcher
{
    public static InternalJobData ReplaceSelected(InternalJobData data, CurrentSelected selected)
        => data with { CurrentSelected = selected };
        
    public static InternalJobData ReplaceSlected(InternalJobData data, SelectNewPairAction action)
        => data with { CurrentSelected = data.CurrentSelected with { Pair = action.Selection } };
        
    public static InternalJobData PatchSortOrder(InternalJobData data, SetSortOrder order)
    {
        var (_, sortOrder) = order;

        if (sortOrder is null) return data;

        if (data.CurrentSelected.Pair is not null && data.CurrentSelected.Pair.Order.Id == sortOrder.Id)
        {
            var pair = data.CurrentSelected.Pair with { Order = sortOrder };
            var selected = data.CurrentSelected with { Pair = pair };
            data = data with { CurrentSelected = selected };
        }
            
        var index = data.CurrentJobs.FindIndex(e => e.Info.Project == sortOrder.Id);
        if (index != -1) 
            data.CurrentJobs[index] = data.CurrentJobs[index] with { Order = sortOrder };

        return data;
    }
}

internal static class IntenalJobDataSelectors
{
    public static CurrentSelected CurrentSelected(InternalJobData data)
        => data.CurrentSelected;
}

public static class InternalDataRequests
{
    public static async Task<CurrentSelected> FetchjobData(CancellationToken cancel, Func<CancellationToken, ValueTask<CurrentSelected>> source, IJobDatabaseService jobDatabaseService)
    {
        var pair = (await source(cancel)).Pair;

        if (pair is null)
            return new CurrentSelected(null, null);

        var jobData = await jobDatabaseService.GetJobData(pair.Info.Project, cancel);

        return new CurrentSelected(pair, jobData);
    }
}