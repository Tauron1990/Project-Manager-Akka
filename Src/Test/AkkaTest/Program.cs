using System;
using System.Collections.Immutable;
using System.IO;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaTest.Feriertage;
using Newtonsoft.Json;
using Tauron.Akka;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Host;

namespace AkkaTest
{
    public sealed class TestActor : ObservableActor
    {

        public TestActor()
        {
            Receive<Start>(
                obs => (from start in obs.Do(_ => Console.WriteLine("Awaiting Signal"))
                        from signal in WaitForSignal<SignalTest>(TimeSpan.FromSeconds(5), _ => true)
                        select "Signaled").Subscribe(
                    m =>
                    {
                        Console.WriteLine(m);
                        ActorApplication.Application.ActorSystem.Terminate();
                    },
                    e =>
                    {
                        Console.WriteLine(e);
                        ActorApplication.Application.ActorSystem.Terminate();
                    }));
        }
    }

    public sealed class TestStart : IStartUpAction
    {
        private readonly ActorSystem _system;

        public TestStart(ActorSystem system)
        {
            _system = system;
        }

        public void Run()
        {
            var test = _system.ActorOf(Props.Create<TestActor>());
            test.Tell(new Start());

            Task.Delay(TimeSpan.FromSeconds(2))
                .ContinueWith(_ => test.Tell(new SignalTest()));
        }
    }

    public record Start;
    public record SignalTest;

    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            Console.Title = "Test Anwendung";

            using HttpClient client = new();
            
            //var serializer = new XmlSerializer(typeof(ArrayOfFeiertagDatum), new []{typeof(FeiertagDatum), typeof(Feiertag), typeof(Laender), typeof(Bundesland) });
            using var result = await client.GetAsync("https://www.spiketime.de/feiertagapi/feiertage/BY/2021");
            string resultString = await result.Content.ReadAsStringAsync();
            await File.WriteAllTextAsync("Test.xml", resultString);
            var data = JsonConvert.DeserializeObject<ImmutableList<Feiertage>>(resultString);
            Console.WriteLine(resultString);

            //await ActorApplication.Create(args)
            //                      .ConfigureAutoFac(cb =>
            //                                        {
            //                                            cb.RegisterType<ConsoleAppRoute>().Named<IAppRoute>("default");
            //                                            cb.RegisterType<TestStart>().As<IStartUpAction>();
            //                                        })
            //                      .Build().Run();
        }
    }
}