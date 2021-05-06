using System;
using Akka.Actor;
using Tauron;
using Tauron.Application.AkkaNode.Services;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.AkkaNode.Services.Reporting;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;
using Tauron.Features;
using Tauron.Operations;

namespace AkkaTest.CommandTest
{
    public sealed class TestApi : ISender
    {
        private readonly IActorRef _targetActor;

        public TestApi(IActorRef targetActor) => _targetActor = targetActor;

        void ISender.SendCommand(IReporterMessage command) => _targetActor.Tell(command);

        public static TestApi Get(IActorRefFactory factory)
            => new(factory.ActorOf("TestApi", Feature.Create(() => new ApiActor(), new EmptyState())));

        private sealed class ApiActor : ReportingActor<EmptyState>
        {
            protected override void ConfigImpl()
            {
                var manager = DataTransferManager.New(Context, "ApiDataManager");
                TryReceive<TestCommand>("Test",
                    obs => obs.ToUnit(m =>
                                      {
                                          var id = Guid.NewGuid().ToString();
                                          manager.Request(DataTransferRequest.FromFile(id, "Program.cs", m.Event.Manager ?? throw new InvalidOperationException("FileManager")));
                                          m.Reporter.Compled(OperationResult.Success(new FileTransactionId(id)));
                                      }));
            }
        }
    }
}