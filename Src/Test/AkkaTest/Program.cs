using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Util.Extensions;
using AkkaTest.JsonRepo;
using Autofac;
using NLog;
using NLog.Config;
using ServiceManager.ProjectDeployment;
using ServiceManager.ProjectRepository;
using SharpRepository.Repository.Configuration;
using Tauron;
using Tauron.Akka;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.AkkaNode.Services.CleanUp;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;
using Tauron.Application.Files.VirtualFiles;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Querys;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Host;
using Tauron.ObservableExt;

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

            var dTest = Math.Round(1d, 0, MidpointRounding.ToPositiveInfinity);

            var test = new Subject<int>();
            var test2 = new Subject<int>();

            var test3 = Observable.When(test.And(test2).Then((i, i1) => i + i1));
            test3.Subscribe(Console.WriteLine);

            test.OnNext(1);
            test.OnNext(2);

            test2.OnNext(1);
            test2.OnNext(2);

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