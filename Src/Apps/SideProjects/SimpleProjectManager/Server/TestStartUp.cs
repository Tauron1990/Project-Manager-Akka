using System.Collections.Immutable;
using Akka.Actor;
using Akka.DependencyInjection;
using SimpleProjectManager.Server.Core;
using SimpleProjectManager.Server.Core.Projections;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server;

#if DEBUG

public static class TestStartUp
{
    public static async void Run(ActorSystem system)
    {
        using var resolver = DependencyResolver.For(system).Resolver.CreateScope();
        var commandProcessor = resolver.Resolver.GetService<CommandProcessor>();

        var testProject = new ProjectName("BM19_12345");

        await Task.Delay(2000);

        var result = await commandProcessor.RunCommand(
            new CreateProjectCommand(
                testProject,
                ImmutableList<ProjectFileId>.Empty.Add(ProjectFileId.For(testProject, new FileName("test,pdf"))),
                ProjectStatus.Running,
                new ProjectDeadline(DateTimeOffset.UtcNow + TimeSpan.FromDays(10))));

        var projector = resolver.Resolver.GetService<ProjectProjectionManager>();
    }
}

#endif