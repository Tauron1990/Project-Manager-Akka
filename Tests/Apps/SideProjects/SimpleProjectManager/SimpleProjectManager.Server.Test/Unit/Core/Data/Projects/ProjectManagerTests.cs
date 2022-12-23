using Akka.Actor;
using Akka.Persistence.TestKit;
using AutoFixture.Xunit2;
using FluentAssertions;
using SimpleProjectManager.Server.Core.Data;
using SimpleProjectManager.Shared.Tests.TestData;
using Tauron.Operations;

namespace SimpleProjectManager.Server.Test.Unit.Core.Data.Projects;

public sealed class ProjectManagerTests : PersistenceTestKit
{
    [Theory, DomainAutoData]
    public async Task Create_Valid_New_Project(CreateProjectCommandCarrier command)
    {
        await WithJournalWrite(
            write => write.Pass(),
            () =>
            {
                IActorRef manager = Manager();

               var result = SendAndGet<OperationResult>(manager, command);
               
               result.Ok.Should().BeTrue();
            });
    }
    
    [Theory, DomainAutoData]
    public async Task Create_Duplicate_Project(CreateProjectCommandCarrier command)
    {
        await WithJournalWrite(
            write => write.Pass(),
            () =>
            {
                IActorRef manager = Manager();

                manager.Tell(command, ActorRefs.NoSender);
                
                var result = SendAndGet<OperationResult>(manager, command);
                
                result.Ok.Should().BeFalse();
                result.Error.Should().NotBeEmpty();
            });
    }

    [Theory, DomainAutoData]
    public async Task Store_Project_Fail(CreateProjectCommandCarrier command)
    {
        await WithJournalWrite(
            b => b.Fail(),
            () =>
            {
                IActorRef manager = Manager();

                var result = SendAndGet<OperationResult>(manager, command);

                result.Ok.Should().BeFalse();
                result.Error.Should().NotBeEmpty();
            });
    }

    [Theory, DomainAutoData]
    public async Task Attach_Files_To_New_Project(ProjectAttachFilesCommandCarrier command)
    {
        await WithJournalWrite(
            b => b.Pass(),
            () =>
            {
                IActorRef manager = Manager();

                var result = SendAndGet<OperationResult>(manager, command);

                result.Ok.Should().BeTrue();
                result.Outcome.Should().Be(true);
            });
    }

    private TResult SendAndGet<TResult>(ICanTell manager, object msg)
    {
        manager.Tell(msg, TestActor);

        return ExpectMsg<TResult>(TimeSpan.FromSeconds(10));
    }

    private IActorRef Manager()
        => ActorOf(() => new ProjectManager());
}