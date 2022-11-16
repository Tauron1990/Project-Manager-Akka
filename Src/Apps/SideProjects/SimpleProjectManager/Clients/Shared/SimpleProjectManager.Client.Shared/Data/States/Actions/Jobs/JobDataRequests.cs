using System;
using System.Threading;
using System.Threading.Tasks;
using SimpleProjectManager.Client.Shared.Data.States.Data;
using SimpleProjectManager.Client.Shared.Services;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Tauron.Operations;

namespace SimpleProjectManager.Client.Shared.Data.States.Actions;

internal static class JobDataRequests
{
    internal static async Task<CurrentSelected> FetchjobData(Func<CancellationToken, ValueTask<CurrentSelected>> source, IJobDatabaseService jobDatabaseService, CancellationToken cancel)
    {
        JobSortOrderPair? pair = (await source(cancel).ConfigureAwait(false)).Pair;

        if(pair is null)
            return new CurrentSelected(Pair: null, JobData: null);

        JobData jobData = await jobDatabaseService.GetJobData(pair.Info.Project, cancel).ConfigureAwait(false);

        return new CurrentSelected(pair, jobData);
    }

    internal static Func<CommitJobEditorData, CancellationToken, ValueTask<SimpleResult>> PostJobCommit(IJobDatabaseService service, IMessageDispatcher messageDispatcher)
    {
        return async (input, token) =>
               {
                   try
                   {
                       JobData newData = input.Commit.JobData.NewData;
                       SimpleResult result = await service.CreateJob(new CreateProjectCommand(newData.JobName, newData.ProjectFiles, newData.Status, newData.Deadline), token).ConfigureAwait(false);

                       if(result.IsError())
                       {
                           input.OnCompled(obj: false);

                           return result;
                       }

                       result = await service.ChangeOrder(new SetSortOrder(IgnoreIfEmpty: true, SortOrder: newData.Ordering), token).ConfigureAwait(false);

                       if(result.IsError())
                       {
                           input.OnCompled(obj: false);

                           return result;
                       }

                       result = await input.Commit.Upload().ConfigureAwait(false);
                       input.OnCompled(result.IsSuccess());

                       return result;
                   }
                   catch (Exception e)
                   {
                       messageDispatcher.PublishError(e);
                       input.OnCompled(obj: false);

                       throw;
                   }
               };
    }
}