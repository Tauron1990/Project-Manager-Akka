using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Akka.Actor;
using Tauron;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.AkkNode.Services.Commands;
using Tauron.Features;

namespace AkkaTest
{
    public record KillMessage;

    public record AlarmMessage;

    public record HelloMessage;

    public record WorldMessage;

    internal static class Program
    {
        public sealed class TestSender : ISender
        {
            public void SendCommand(IReporterMessage command) { }
        }

        public sealed class TestCommand : ResultCommand<TestSender, TestCommand, string>
        {
            protected override string Info => nameof(TestCommand);
        }

        private sealed class KillFeature : IFeature<EmptyState>
        {
            public IEnumerable<string> Identify()
            {
                yield return nameof(KillFeature);
            }

            public void Init(IFeatureActor<EmptyState> actor) 
                => actor.Receive<KillMessage>(obs => obs.SubscribeWithStatus(_ => actor.Context.System.Terminate()));
        }

        private sealed class HelloFeature : IFeature<EmptyState>
        {
            public IEnumerable<string> Identify()
            {
                yield return nameof(HelloFeature);
            }

            public void Init(IFeatureActor<EmptyState> actor)
            {
                actor.Receive<AlarmMessage>(obs => obs.SubscribeWithStatus(_ => Console.WriteLine("Here Is Hello Feature")));
                actor.Receive<HelloMessage>(obs => obs.SubscribeWithStatus(_ => Console.Write("Hello ")));
            }
        }

        private sealed class WorldFeature : IFeature<EmptyState>
        {
            public IEnumerable<string> Identify()
            {
                yield return nameof(WorldFeature);
            }

            public void Init(IFeatureActor<EmptyState> actor)
            {
                actor.Receive<AlarmMessage>(obs => obs.SubscribeWithStatus(_ => Console.WriteLine("Here Is World Feature")));
                actor.Receive<WorldMessage>(obs => obs.SubscribeWithStatus(_ => Console.WriteLine("World!")));
            }
        }

        private static async Task Main()
        {
            var testSender = new TestSender();
            var testCommand = new TestCommand();
            
            testSender.SendResult(testCommand, TimeSpan.FromHours(1), Console.WriteLine).Wait();

            using var system = ActorSystem.Create("Test");

            var test = system.ActorOf(Feature.Create(() => new KillFeature(), () => new HelloFeature(), () => new WorldFeature()));
            test.Tell(new AlarmMessage());
            test.Tell(new HelloMessage());
            test.Tell(new WorldMessage());
            test.Tell(new KillMessage());

            await system.WhenTerminated;
        }
    }
}