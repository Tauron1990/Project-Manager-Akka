using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaTest.InMemoryStorage;
using Autofac;
using Serilog;
using ServiceManager.ProjectRepository;
using SharpRepository.InMemoryRepository;
using SharpRepository.Repository.Configuration;
using Tauron.Akka;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.AkkaNode.Services.CleanUp;
using Tauron.Application.AkkaNode.Services.Commands;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;
using Tauron.Application.Files.VirtualFiles;
using Tauron.Application.Files.VirtualFiles.InMemory.Data;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Features;
using Tauron.Host;
using Tauron.Temp;

namespace AkkaTest
{
    //public sealed class HalloWeltActor : ReceiveActor
    //{
    //    public HalloWeltActor(ILifetimeScope scope) => Receive<string>(c => Console.WriteLine($"Hallo: {c}"));
    //}

    //public sealed record MyClass(string Id, string Value)
    //{
    //    public MyClass()
    //        : this("", "") { }
    //}

    public sealed record Start;

    //public sealed class TestActor : ReceiveActor
    //{
    //    private const string TestRepo = "Tauron1990/Radio-Streamer";

    //    private readonly DataTransferManager _dataTransfer;
    //    private readonly DataDirectory _bucked;
    //    private readonly RepositoryApi _repositoryApi;
    //    private readonly string OpsId = Guid.NewGuid().ToString("N");

    //    private readonly ActorSystem _system;

    //    public TestActor(RepositoryManager repositoryManager, DataTransferManager dataTransfer, DataDirectory bucked)
    //    {
    //        _dataTransfer = dataTransfer;
    //        _bucked = bucked;
    //        _repositoryApi = RepositoryApi.CreateFromActor(repositoryManager.Manager);

    //        _system = Context.System;
    //        Receive<Start>(_ => Task.Run(Start));
    //    }

    //    private async Task Start()
    //    {
    //        try
    //        {
    //            var bucked = _bucked;
    //            var data = PersistentInMemorxConfigRepositoryFactory.Repositorys;

    //            Console.WriteLine("Test 1:");

    //            await _repositoryApi.Send(new RegisterRepository(TestRepo, true), TimeSpan.FromMinutes(30), Console.WriteLine);
    //            Console.WriteLine("Test 1 Erfolgreich...");

    //            Console.WriteLine("Test 2:");
    //            var result = await _repositoryApi.Send(new TransferRepository(TestRepo, OpsId), TimeSpan.FromMinutes(30), _dataTransfer, Console.WriteLine, () => File.Create("Test.zip"));

    //            switch (result)
    //            {
    //                case TransferFailed f:
    //                    Console.WriteLine("Test 2 Gescheitert...");
    //                    Console.WriteLine(f.Reason);
    //                    break;
    //                case TransferSucess:
    //                    Console.WriteLine("Test 2 Erfolgreich...");
    //                    Process.Start(new ProcessStartInfo(Path.GetFullPath("Test.zip")) {UseShellExecute = true});
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
    //            #pragma warning disable 4014
    //            _system.Terminate();
    //            #pragma warning restore 4014
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
    //        var bucked = factory.CreateInMemory("Test", rootDic);

    //        var config = new SharpRepositoryConfiguration();

    //        //config.AddRepository(new MongoDbRepositoryConfiguration(CleanUpManager.RepositoryKey, con){ Factory = typeof(MongoDbConfigRepositoryFactory) });
    //        config.AddRepository(new InMemoryRepositoryConfiguration(RepositoryManager.RepositoryManagerKey) { Factory = typeof(PersistentInMemorxConfigRepositoryFactory) });

    //        var dataManager = DataTransferManager.New(_system);
    //        var manager = RepositoryManager.CreateInstance(_system, new RepositoryManagerConfiguration(config, bucked, dataManager));

    //        _system.ActorOf(Props.Create(() => new TestActor(manager, dataManager, rootDic))).Tell(new Start());
    //    }
    //}


    public sealed class TestStart : IStartUpAction
    {
        private readonly ActorSystem _testSytem;
        private readonly string TestOp = Guid.NewGuid().ToString("D");

        public TestStart(ActorSystem testSytem) => _testSytem = testSytem;

        public async void Run()
        {
            try
            {
                var source = DataTransferManager.New(_testSytem);
                var target = DataTransferManager.New(_testSytem);

                source.Request(DataTransferRequest.FromStream(TestOp, () => File.OpenRead("Program.cs"), target, "Test"));

                var result = await target.AskAwaitOperation(new AwaitRequest(TimeSpan.FromMinutes(30), TestOp));

                switch (await result.TryStart(() => new StreamData(File.OpenWrite("Program.txt"))))
                {
                    case TransferFailed f:
                        Console.WriteLine($"Test Failed: {f.Reason}");
                        break;
                    case TransferSucess:
                        Console.WriteLine("Test Sucess");
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Transfer Failed");
                Console.WriteLine(e);
            }

        }
    }

    internal static class Program
    {
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