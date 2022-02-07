using System.Collections.Immutable;
using SimpleProjectManager.Client.ViewModels;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Tauron;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Data.States;

public record SelectNewPairAction(JobSortOrderPair? Selection);

public record CommitJobEditorData(JobEditorCommit Commit, Action<bool> OnCompled);

internal static class JobDataPatcher
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

    public static InternalJobData PatchEditorCommit(InternalJobData data, CommitJobEditorData input)
    {
        var toPatch = input.Commit.JobData.NewData;
        var newInfo = new JobInfo(toPatch.Id, toPatch.JobName, toPatch.Deadline, toPatch.Status, toPatch.ProjectFiles.Count != 0);
        
        var jobIndex = data.CurrentJobs.FindIndex(p => p.Info.Project == toPatch.Id);
        if(jobIndex == -1)
        {
            return data with
                   {
                       CurrentJobs =
                       ImmutableList<JobSortOrderPair>.Empty
                          .AddRange(data.CurrentJobs)
                          .Add(new JobSortOrderPair(
                               toPatch.Ordering ?? new SortOrder(toPatch.Id, 0, false),
                               newInfo))
                          .ToArray()
                   };
        }

        var job = data.CurrentJobs[jobIndex];

        if (toPatch.Ordering is not null) 
            job = job with { Order = toPatch.Ordering, Info = newInfo};
        else
            job = job with { Info = newInfo };
        
        data.CurrentJobs[jobIndex] = job;
        return data;
    }
}

internal static class JobDataSelectors
{
    public static CurrentSelected CurrentSelected(InternalJobData data)
        => data.CurrentSelected;
}

internal static class JobDataRequests
{
    public static async Task<CurrentSelected> FetchjobData(Func<CancellationToken, ValueTask<CurrentSelected>> source, IJobDatabaseService jobDatabaseService, CancellationToken cancel)
    {
        var pair = (await source(cancel)).Pair;

        if (pair is null)
            return new CurrentSelected(null, null);

        var jobData = await jobDatabaseService.GetJobData(pair.Info.Project, cancel);

        return new CurrentSelected(pair, jobData);
    }

    public static Func<CommitJobEditorData, CancellationToken, ValueTask<string?>> PostJobCommit(IJobDatabaseService service, IEventAggregator aggregator)
    {
        return async (input, token) =>
               {
                   try
                   {
                       var newData = input.Commit.JobData.NewData;
                       var result = await service.CreateJob(new CreateProjectCommand(newData.JobName, newData.ProjectFiles, newData.Status, newData.Deadline), token);

                       if (!string.IsNullOrWhiteSpace(result))
                       {
                           input.OnCompled(false);
                           return result;
                       }

                       result = await service.ChangeOrder(new SetSortOrder(true, newData.Ordering), token);

                       if (!string.IsNullOrWhiteSpace(result))
                       {
                           input.OnCompled(false);
                           return result;
                       }

                       result = await input.Commit.Upload();
                       input.OnCompled(string.IsNullOrWhiteSpace(result));

                       return result;
                   }
                   catch (Exception e)
                   {
                       aggregator.PublishError(e);
                       input.OnCompled(false);
                       throw;
                   }
               };
    }
}