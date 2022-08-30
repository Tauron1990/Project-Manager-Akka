using Akka.Actor;
using Akka.DependencyInjection;
using SimpleProjectManager.Server.Core.Tasks;
using Tauron.Application.AkkaNode.Bootstrap;

namespace SimpleProjectManager.Server.Core.JobManager;

public sealed class JobManagerRegistrations
{
    private readonly TaskManagerCore _taskManagerCore;
    private readonly IDependencyResolver _dependencyResolver;
    
    public JobManagerRegistrations(TaskManagerCore taskManagerCore, ActorSystem actorSystem)
    {
        _taskManagerCore = taskManagerCore;
        var ext = DependencyResolver.For(actorSystem);
        
        _dependencyResolver = ext.Resolver;
    }

    public void Run()
    {
        _taskManagerCore
           .Register<FilePurgeManager, FilePureScheduler, FilePurgeRunner, FilePurgeJob, FilePurgeId>("File Purge", _dependencyResolver);
    }
}