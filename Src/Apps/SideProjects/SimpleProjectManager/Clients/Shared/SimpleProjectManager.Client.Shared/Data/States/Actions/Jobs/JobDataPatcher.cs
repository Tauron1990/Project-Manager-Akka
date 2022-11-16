using System.Collections.Immutable;
using System.Linq;
using SimpleProjectManager.Client.Shared.Data.States.Data;
using SimpleProjectManager.Shared.Services;
using Tauron;

namespace SimpleProjectManager.Client.Shared.Data.States.Actions;

internal static class JobDataPatcher
{
    internal static InternalJobData ReplaceSelected(InternalJobData data, CurrentSelected selected)
        => data with { CurrentSelected = selected };

    internal static InternalJobData ReplaceSlected(InternalJobData data, SelectNewPairAction action)
        => data.CurrentSelected is null
            ? data with { CurrentSelected = new CurrentSelected(data.FindPair(action.Selection), JobData: null) }
            : data with { CurrentSelected = data.CurrentSelected with { Pair = data.FindPair(action.Selection) } };
    
    internal static InternalJobData PatchSortOrder(InternalJobData data, SetSortOrder order)
    {
        SortOrder? sortOrder = order.SortOrder;

        if(sortOrder is null) return data;

        if(data.CurrentSelected?.Pair is not null && data.CurrentSelected.Pair.Order.Id == sortOrder.Id)
        {
            JobSortOrderPair pair = data.CurrentSelected.Pair with { Order = sortOrder };
            CurrentSelected selected = data.CurrentSelected with { Pair = pair };
            data = data with { CurrentSelected = selected };
        }

        int index = data.CurrentJobs.FindIndex(e => e.Info.Project == sortOrder.Id);
        if(index != -1)
            data.CurrentJobs[index] = data.CurrentJobs[index] with { Order = sortOrder };

        return data;
    }

    internal static InternalJobData PatchEditorCommit(InternalJobData data, CommitJobEditorData input)
    {
        JobData toPatch = input.Commit.JobData.NewData;
        var newInfo = new JobInfo(toPatch.Id, toPatch.JobName, toPatch.Deadline, toPatch.Status, toPatch.ProjectFiles.Count != 0);

        int jobIndex = data.CurrentJobs.FindIndex(p => p.Info.Project == toPatch.Id);

        if(jobIndex == -1)
            return data with
                   {
                       CurrentJobs =
                       ImmutableList<JobSortOrderPair>.Empty
                          .AddRange(data.CurrentJobs)
                          .Add(
                               new JobSortOrderPair(
                                   toPatch.Ordering ?? new SortOrder
                                                       {
                                                           Id = toPatch.Id,
                                                           SkipCount = 0,
                                                           IsPriority = false
                                                       },
                                   newInfo))
                          .ToArray()
                   };

        JobSortOrderPair job = data.CurrentJobs[jobIndex];

        if(toPatch.Ordering is not null)
            job = job with { Order = toPatch.Ordering, Info = newInfo };
        else
            job = job with { Info = newInfo };

        data.CurrentJobs[jobIndex] = job;

        return data;
    }
}