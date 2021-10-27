using System.Collections.Immutable;
using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Persistence;
using SimpleProjectManager.Server.Core;
using SimpleProjectManager.Server.Core.Projections;
using SimpleProjectManager.Shared;
using Tauron.Application.AkkaNode.Bootstrap;

namespace SimpleProjectManager.Server;

#if DEBUG

public static class TestStartUp
{
    public static async void Run(ActorSystem system)
    {
        using var resolver = DependencyResolver.For(system).Resolver.CreateScope();
        var projector = resolver.Resolver.GetService<ProjectProjectionManager>();
        //var commandProcessor = resolver.Resolver.GetService<CommandProcessor>();

        //var testProject = new ProjectName("BM19_12345");

        //var result = await commandProcessor.RunCommand(
        //    new CreateProjectCommand(
        //        testProject,
        //        ImmutableList<ProjectFileId>.Empty.Add(ProjectFileId.For(testProject, new FileName("test,pdf"))),
        //        ProjectStatus.Running,
        //        new ProjectDeadline(DateTimeOffset.UtcNow + TimeSpan.FromDays(10))));
    }
}

#endif