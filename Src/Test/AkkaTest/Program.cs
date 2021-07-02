using System;
using System.Reactive.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Akka.Actor;
using Akka.Configuration;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

    public record Test(string Value);

    public record Test1(string Value);

    public sealed class TestProfile : Profile
    {
        public TestProfile()
        {
            CreateMap<Test, Test1>();
        }
    }

    internal static class Program
    {
        private static void Main()
        {
            Console.Title = "Test Anwendung";
            
            var coll = new ServiceCollection();
            coll.AddAutoMapper(c => c.AddProfile(new TestProfile()));
            using var test = coll.BuildServiceProvider();

            var result = test.GetRequiredService<IMapper>().Map<Test1>(new Test("Hallo Welt"));

            //var dataTest = new DataManager(new ConcurancyManager(), new EventAggregator());
            //var profileTest = new ProfileManager(dataTest);
            //var calcTest = new CalculationManager(profileTest, SystemClock.Inst);

            //(from res in calcTest.CalculationResult
            // select res)
            //   .Subscribe(Console.WriteLine);

            //(from hour in calcTest.AllHours
            // select hour.Hours).Subscribe(Console.WriteLine);

            //dataTest.Mutate(pd => Observable.Return(new ProfileData("Test.json", 173, 18, 0,
            //                    ImmutableDictionary<DateTime, ProfileEntry>.Empty,
            //                    DateTime.Now.Date, ImmutableList<HourMultiplicator>.Empty, false, 8))).Subscribe();

            //profileTest.AddEntry(new ProfileEntry(DateTime.Today, null, null, DayType.Holiday)).Subscribe(u => Console.WriteLine(u));

            //await ActorApplication.Create(args)
            //                      .ConfigureAutoFac(cb =>
            //                                        {
            //                                            cb.RegisterType<ConsoleAppRoute>().Named<IAppRoute>("default");
            //                                            cb.RegisterType<TestStart>().As<IStartUpAction>();
            //                                        })
            //                      .Build().Run();

            Console.ReadKey();
        }

        private static void Test(Channel<string> channel)
        {
            channel.Writer.WriteAsync("test");
        }
    }
}