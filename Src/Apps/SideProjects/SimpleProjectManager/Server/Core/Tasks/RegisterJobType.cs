using Akka.Actor;
using Akkatecture.Jobs;
using Akkatecture.Jobs.Commands;
using Tauron;
using Tauron.Operations;

namespace SimpleProjectManager.Server.Core.Tasks;

public sealed record RegisterJobType(
    string Id, Func<Props> JobManagerFactory, Func<object, bool> IsCompatible, 
    Func<string, object> CreateCancel, Func<object, string> GetId)
{
    public static RegisterJobType Create<TJobManager, TJobScheduler, TJobRunner, TJob, TId>(string id)
        where TJobManager : JobManager<TJobScheduler, TJobRunner, TJob, TId>, new()
        where TJob : IJob
        where TId : IJobId
        where TJobRunner : JobRunner<TJob, TId>
        where TJobScheduler : JobScheduler<TJobScheduler, TJob, TId>
        => new(
            id,
            () => Props.Create(() => new TJobManager()),
            o => o is SchedulerCommand<TJob, TId>,
            s => new Cancel<TJob, TId>(FastReflection.Shared.FastCreateInstance<TId>(s), OperationResult.Success(), OperationResult.Failure()),
            o => o switch
            {
                Schedule<TJob, TId> schedule => schedule.ToString(),
                TId jobId => jobId.ToString() ?? string.Empty,
                _ => string.Empty
            });
}