using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimpleProjectManager.Client.Shared.Data.JobEdit;
using SimpleProjectManager.Client.Shared.Data.States.Data;
using SimpleProjectManager.Client.Shared.Services;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Tauron;
using Tauron.Operations;

namespace SimpleProjectManager.Client.Shared.Data.States.Actions;

public record SelectNewPairAction(PairSelection Selection);

public record CommitJobEditorData(JobEditorCommit Commit, Action<bool> OnCompled);

internal static class JobDataPatcher
{
    public static InternalJobData ReplaceSelected(InternalJobData data, CurrentSelected selected)
        => data with { CurrentSelected = selected };

    public static InternalJobData ReplaceSlected(InternalJobData data, SelectNewPairAction action)
        => data.CurrentSelected is null
            ? data with { CurrentSelected = new CurrentSelected(data.FindPair(action.Selection), null) }
            : data with { CurrentSelected = data.CurrentSelected with { Pair = data.FindPair(action.Selection) } };

    public static InternalJobData PatchSortOrder(InternalJobData data, SetSortOrder order)
    {
        (_, SortOrder? sortOrder) = order;

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

    public static InternalJobData PatchEditorCommit(InternalJobData data, CommitJobEditorData input)
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

internal static class JobDataSelectors
{
    public static CurrentSelected CurrentSelected(InternalJobData data)
        => data.CurrentSelected ?? new CurrentSelected(null, null);
}

internal static class JobDataRequests
{
    public static async Task<CurrentSelected> FetchjobData(Func<CancellationToken, ValueTask<CurrentSelected>> source, IJobDatabaseService jobDatabaseService, CancellationToken cancel)
    {
        JobSortOrderPair? pair = (await source(cancel)).Pair;

        if(pair is null)
            return new CurrentSelected(null, null);

        JobData jobData = await jobDatabaseService.GetJobData(pair.Info.Project, cancel);

        return new CurrentSelected(pair, jobData);
    }

    public static Func<CommitJobEditorData, CancellationToken, ValueTask<SimpleResult>> PostJobCommit(IJobDatabaseService service, IMessageDispatcher messageDispatcher)
    {
        return async (input, token) =>
               {
                   try
                   {
                       JobData newData = input.Commit.JobData.NewData;
                       SimpleResult result = await service.CreateJob(new CreateProjectCommand(newData.JobName, newData.ProjectFiles, newData.Status, newData.Deadline), token);

                       if(result.IsError())
                       {
                           input.OnCompled(false);

                           return result;
                       }

                       result = await service.ChangeOrder(new SetSortOrder(true, newData.Ordering), token);

                       if(result.IsError())
                       {
                           input.OnCompled(false);

                           return result;
                       }

                       result = await input.Commit.Upload();
                       input.OnCompled(result.IsSuccess());

                       return result;
                   }
                   catch (Exception e)
                   {
                       messageDispatcher.PublishError(e);
                       input.OnCompled(false);

                       throw;
                   }
               };
    }
}