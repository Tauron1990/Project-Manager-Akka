using Akka.Actor;
using Akka.DependencyInjection;
using SimpleProjectManager.Server.Core.Tasks;

namespace SimpleProjectManager.Server.Core.JobManager;

public sealed class JobManagerRegistrations
{
    private readonly IDependencyResolver _dependencyResolver;
    private readonly TaskManagerCore _taskManagerCore;

    public JobManagerRegistrations(TaskManagerCore taskManagerCore, ActorSystem actorSystem)
    {
        _taskManagerCore = taskManagerCore;
        DependencyResolver? ext = DependencyResolver.For(actorSystem);

        _dependencyResolver = ext.Resolver;
    }

    public void Run()
    {
        _taskManagerCore
           .Register<FilePurgeManager, FilePureScheduler, FilePurgeRunner, FilePurgeJob, FilePurgeId>("File Purge", _dependencyResolver);
    }
}