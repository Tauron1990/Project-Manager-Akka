using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Akka.Actor;
using Tauron;
using Tauron.Features;

namespace AkkaTest
{
    public record KillMessage;

    public record AlarmMessage;

    public record HelloMessage;

    public record WorldMessage;

    internal static class Program
    {
        private sealed class KillFeature : IFeature<EmptyState>
        {
            public void Init(IFeatureActor<EmptyState> actor) 
                => actor.Receive<KillMessage>(obs => obs.SubscribeWithStatus(_ => actor.Context.System.Terminate()));
        }

        private sealed class HelloFeature : IFeature<EmptyState>
        {
            public void Init(IFeatureActor<EmptyState> actor)
            {
                actor.Receive<AlarmMessage>(obs => obs.SubscribeWithStatus(_ => Console.WriteLine("Here Is Hello Feature")));
                actor.Receive<HelloMessage>(obs => obs.SubscribeWithStatus(_ => Console.Write("Hello ")));
            }
        }

        private sealed class WorldFeature : IFeature<EmptyState>
        {
            public void Init(IFeatureActor<EmptyState> actor)
            {
                actor.Receive<AlarmMessage>(obs => obs.SubscribeWithStatus(_ => Console.WriteLine("Here Is World Feature")));
                actor.Receive<WorldMessage>(obs => obs.SubscribeWithStatus(_ => Console.WriteLine("World!")));
            }
        }

        private static async Task Main()
        {
            using var system = ActorSystem.Create("Test");

            var test = system.ActorOf(Feature.Create(new KillFeature(), new HelloFeature(), new WorldFeature()));
            test.Tell(new AlarmMessage());
            test.Tell(new HelloMessage());
            test.Tell(new WorldMessage());
            test.Tell(new KillMessage());

            await system.WhenTerminated;
        }
    }
}