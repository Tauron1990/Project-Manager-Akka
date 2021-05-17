using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Actor;
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

namespace AkkaTest
{
    public sealed class TestActor : ObservableActor
    {
        public TestActor()
        {
            IObservable<TType> Sync<TType>(TType input)
                => Observable.Return(input, ActorScheduler.From(Self));

            Receive<Start>(
                obs => (from _ in obs
                       from asyncTest in Task.Run(async () =>
                                                  {
                                                      await Task.Delay(1000);
                                                      return "Hallo Welt";
                                                  })
                       from syncTest in Sync(asyncTest)
                       select (Context.Self, syncTest)
                       ).Subscribe(n => Console.WriteLine($"{n.syncTest} -- {n.Self.Path}"), e => Console.WriteLine(e)));
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
            _system.ActorOf(Props.Create<TestActor>()).Tell(new Start());
        }
    }

    public record Start;
    
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            Console.Title = "Test Anwendung";
            

            await ActorApplication.Create(args)
                                  .ConfigureAutoFac(cb =>
                                                    {
                                                        cb.RegisterType<ConsoleAppRoute>().Named<IAppRoute>("default");
                                                        cb.RegisterType<TestStart>().As<IStartUpAction>();
                                                    })
                                  .Build().Run();
        }
    }
}