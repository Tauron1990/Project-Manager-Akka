using System;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Tools.PublishSubscribe;
using Spectre.Console;
using Tauron.Application.AkkaNode.Bootstrap;

namespace AkkaTest.FusionTest.Seed
{
    public class SeedStarter : IStartUpAction
    {
        private readonly ActorSystem _system;

        public SeedStarter(ActorSystem system)
            => _system = system;

        public void Run()
        {
            _system.ActorOf(Props.Create<MessageReciver>());
            var cluster = Cluster.Get(_system);
            cluster.JoinAsync(cluster.SelfAddress)
                   .ContinueWith(
                        async t =>
                        {
                            if (t.IsCompletedSuccessfully)
                                await Task.Delay(5000);
                            else
                                AnsiConsole.WriteException(t.Exception?.Flatten()?.InnerExceptions.FirstOrDefault() ?? new InvalidOperationException("Unbkannter Fehler"));
                        })
                   .ContinueWith(
                        _ =>
                        {
                            Console.Title = "Seed Server";
                            
                            AnsiConsole.Clear();
                            AnsiConsole.Render(new Rule("Setup Abgeschlossen"));
                            AnsiConsole.WriteLine();
                            AnsiConsole.WriteLine($"Eigene Adresse: {cluster.SelfAddress}");
                            AnsiConsole.WriteLine($"Strg-c zum Beenden");
                        });
        }
        
        
        private sealed class MessageReciver : ReceiveActor
        {
            public MessageReciver()
            {
                var mediator = DistributedPubSub.Get(Context.System).Mediator;
                mediator.Tell(new Subscribe(MessageChannel.ChannelId, Self));

                Receive<string>(msg =>
                                {
                                    AnsiConsole.WriteLine();
                                    AnsiConsole.WriteLine($"Extenal: {msg}");
                                });
            }
        }
    }
}