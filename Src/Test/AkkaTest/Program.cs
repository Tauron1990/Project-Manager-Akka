using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Actor.Internal;
using Akka.Util;
using AkkaTest.CommandTest;
using AkkaTest.InMemoryStorage;
using Autofac;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Serilog;
using Serilog.Events;
using ServiceManager.ProjectRepository;
using SharpRepository.InMemoryRepository;
using SharpRepository.MongoDbRepository;
using SharpRepository.Repository.Configuration;
using Tauron.Akka;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.AkkaNode.Services.CleanUp;
using Tauron.Application.AkkaNode.Services.Commands;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;
using Tauron.Application.Files.GridFS;
using Tauron.Application.Files.VirtualFiles;
using Tauron.Application.Files.VirtualFiles.InMemory.Data;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Host;

namespace AkkaTest
{
    //public sealed class TestActor : ReceiveActor
    //{
    //    private const string TestRepo = "Tauron1990/Radio-Streamer";

    //    private readonly DataTransferManager _dataTransfer;
    //    private readonly DataDirectory _bucked;
    //    private readonly RepositoryApi _repositoryApi;

    //    private readonly ActorSystem _system;

    //    public TestActor(RepositoryManager repositoryManager, DataDirectory bucked)
    //    {
    //        _dataTransfer = DataTransferManager.New(Context, "TargetDataTransfer");
    //        _bucked = bucked;
    //        _repositoryApi = RepositoryApi.CreateFromActor(repositoryManager.Manager);

    //        _system = Context.System;
    //        ReceiveAsync<Start>(Start);
    //    }

    //    private async Task Start(Start s)
    //    {
    //        try
    //        {
    //            var bucked = _bucked;
    //            var data = PersistentInMemorxConfigRepositoryFactory.Repositorys;

    //            Console.WriteLine("Test 1:");

    //            await _repositoryApi.Send(new RegisterRepository(TestRepo, true), TimeSpan.FromMinutes(30), Log.Information);
    //            Console.WriteLine("Test 1 Erfolgreich...");

    //            Console.WriteLine("Test 2:");
    //            var result = await _repositoryApi.Send(new TransferRepository(TestRepo), TimeSpan.FromMinutes(30), _dataTransfer, Log.Information, () => File.Create("Test.zip"));

    //            switch (result)
    //            {
    //                case TransferFailed f:
    //                    Console.WriteLine("Test 2 Gescheitert...");
    //                    Console.WriteLine(f.Reason);
    //                    break;
    //                case TransferSucess:
    //                    Console.WriteLine("Test 2 Erfolgreich...");
    //                    Process.Start(new ProcessStartInfo(Path.GetFullPath("Test.zip")) { UseShellExecute = true });
    //                    break;
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            Console.WriteLine("Test Fehlgeschlagen...");
    //            Console.WriteLine(e);
    //            throw;
    //        }
    //        finally
    //        {
    //            await _system.Terminate();
    //        }
    //    }
    //}

    //public sealed class TestStart : IStartUpAction
    //{
    //    private readonly ActorSystem _system;

    //    public TestStart(ActorSystem system)
    //    {
    //        _system = system;
    //    }

    //    public void Run()
    //    {
    //        var rootDic = new DataDirectory("Test");
    //        var factory = new VirtualFileFactory();
    //        var bucked = factory.CreateMongoDb(new GridFSBucket(new MongoClient(MongoUrl.Create(Program.con)).GetDatabase("TestDb")));

    //        var config = new SharpRepositoryConfiguration();

    //        config.AddRepository(new MongoDbRepositoryConfiguration(RepositoryManager.RepositoryManagerKey, Program.con)
    //        {
    //            Factory = typeof(MongoDbConfigRepositoryFactory)
    //        });
    //        //config.AddRepository(new InMemoryRepositoryConfiguration(RepositoryManager.RepositoryManagerKey) { Factory = typeof(PersistentInMemorxConfigRepositoryFactory) });

    //        var dataManager = DataTransferManager.New(_system, "RepoDataTransfer");
    //        var manager = RepositoryManager.CreateInstance(_system, new RepositoryManagerConfiguration(config, bucked, dataManager));

    //        _system.ActorOf(Props.Create(() => new TestActor(manager, rootDic)), "Start_Helper").Tell(new Start());
    //    }
    //}

    public record Start;

    public sealed class TestStart : IStartUpAction
    {
        private sealed class TestStop : SupervisorStrategy
        {
            protected override Directive Handle(IActorRef child, Exception exception)
            {
                return Directive.Stop;
            }

            public override void ProcessFailure(IActorContext context, bool restart, IActorRef child, Exception cause, ChildRestartStats stats, IReadOnlyCollection<ChildRestartStats> children)
            {

            }

            public override void HandleChildTerminated(IActorContext actorContext, IActorRef child, IEnumerable<IInternalActorRef> children)
            {
            }

            public override ISurrogate ToSurrogate(ActorSystem system) => null!;

            public override IDecider Decider => DefaultDecider;
        }

        private sealed class Watcher : ReceiveActor
        {
            public Watcher()
            {
                ReceiveAsync<Start>(Start);
            }

            protected override SupervisorStrategy SupervisorStrategy() => new TestStop();

            private async Task Start(Start obj)
            {
                try
                {
                    var target = DataTransferManager.New(Context, "Target");
                    var api = TestApi.Get(Context);

                    for (int i = 0; i < 10; i++)
                    {
                        try
                        {
                            Log.Information($"Start Test {i}");

                            var res = await api.Send(new TestCommand(), TimeSpan.FromMinutes(1), target, Log.Information, () => File.OpenWrite("Program.txt"));

                            //Thread.Sleep(1000);

                            switch (res)
                            {
                                case TransferFailed f:
                                    Log.Information($"Test Failed: {i} {f.Reason}");
                                    break;
                                case TransferSucess:
                                    Log.Information($"Test Sucess: {i}");
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Information($"Transfer Failed {i}");
                            Console.WriteLine(e);
                        }
                    }
                }
                finally
                {
                    await Context.System.Terminate();
                }
            }
        }

        private readonly ActorSystem _testSytem;

        public TestStart(ActorSystem testSytem) => _testSytem = testSytem;

        public void Run()
        {
            _testSytem.ActorOf<Watcher>("Watcher").Tell(new Start());
        }
    }

    internal static class Program
    {
        public const string con = "mongodb://192.168.105.96:27017/TestDb?readPreference=primary&appname=MongoDB%20Compass%20Community&ssl=false";
        //public const string con = "mongodb://localhost:27017/TestDb?readPreference=primary&appname=MongoDB%20Compass%20Community&ssl=false";

        private static async Task Main(string[] args)
        {
            Console.Title = "Test Anwendung";

            //const string con = "mongodb://192.168.105.96:27017/TestDb?readPreference=primary&appname=MongoDB%20Compass%20Community&ssl=false";
            ////const string con = "mongodb://localhost:27017/TestDb?readPreference=primary&appname=MongoDB%20Compass%20Community&ssl=false";

            await ActorApplication.Create(args)
                                  .ConfigureLogging((_, c) => c.WriteTo.Console())
                                  .ConfigureAutoFac(cb =>
                                                    {
                                                        cb.RegisterType<ConsoleAppRoute>().Named<IAppRoute>("default");
                                                        cb.RegisterType<TestStart>().As<IStartUpAction>();
                                                    })
                                  .Build().Run();
        }
    }
}