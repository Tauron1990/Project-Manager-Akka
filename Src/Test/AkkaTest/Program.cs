using System;
using System.Diagnostics;
using System.IO;
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
    public sealed class TestActor : ReceiveActor
    {
        private const string TestRepo = "Tauron1990/Project-Manager-Akka";
        private const string TestProject = "AkkaTest.csproj";

        private readonly DataTransferManager _dataTransfer;
        private readonly DeploymentApi _deploymentApi;
        private readonly RepositoryApi _repositoryApi;
        private readonly IVirtualFileSystem _bucked;

        private readonly ActorSystem _system;

        public TestActor(ISharpRepositoryConfiguration repositoryConfiguration, IVirtualFileSystem bucked)
        {
            _bucked = bucked;
            _dataTransfer = DataTransferManager.New(Context, "Test_Transfer");
            
            _repositoryApi = RepositoryApi.CreateFromActor(
                RepositoryManager.CreateInstance(Context, 
                    new RepositoryManagerConfiguration(repositoryConfiguration, bucked.GetDirectory("Repository_Test"), DataTransferManager.New(Context, "Repository_Transfer")))
                                 .Manager);

            _deploymentApi = DeploymentApi.CreateFromActor(
                DeploymentManager.CreateInstance(Context,
                                      new DeploymentConfiguration(repositoryConfiguration, bucked.GetDirectory("Deployment_Test"), DataTransferManager.New(Context, "Deployment_Tramsfer"), _repositoryApi))
                                 .Manager);

            _system = Context.System;
            ReceiveAsync<Start>(Start);
        }

        private async Task Start(Start s)
        {
            try
            {
                var file = _bucked.GetFile("build.zip");
                var dic = _bucked.GetDirectory("Build");

                var data = await _deploymentApi.Send(new QueryBinarys("TestApp1"), TimeSpan.FromMinutes(10), _dataTransfer, Console.WriteLine, () => File.Create("Test.zip"));

                switch (data)
                {
                    case TransferSucess:
                        Process.Start(new ProcessStartInfo(Path.GetFullPath("Test.zip")) {UseShellExecute = true});
                        Console.WriteLine($"Test Erfolgreich.");
                        break;
                    case TransferFailed f:
                        Console.WriteLine("Test Failed");
                        Console.WriteLine(f.Reason);
                        break;
                }


            }
            catch (Exception e)
            {
                Console.WriteLine("Test Fehlgeschlagen...");
                Console.WriteLine(e);
            }
            finally
            {
                await _system.Terminate();
            }
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
            var rootDic = Path.GetFullPath("Test");
            var factory = new VirtualFileFactory();
            var bucked = factory.CrerateLocal(Path.Combine(rootDic));
            var dbPath = bucked.GetDirectory("DB").OriginalPath;
            dbPath.CreateDirectoryIfNotExis();

            var config = new SharpRepositoryConfiguration();

            config.AddRepository(new JsonRepositoryConfiguration(DeploymentManager.RepositoryKey, dbPath));
            config.AddRepository(new JsonRepositoryConfiguration(RepositoryManager.RepositoryKey, dbPath));
            config.AddRepository(new JsonRepositoryConfiguration(CleanUpManager.RepositoryKey, dbPath));
            //config.AddRepository(new InMemoryRepositoryConfiguration(RepositoryManager.RepositoryKey) { Factory = typeof(PersistentInMemorxConfigRepositoryFactory) });

            _system.ActorOf(Props.Create(() => new TestActor(config, bucked)), "Start_Helper").Tell(new Start());
        }
    }

    public record Start;

    //public sealed class TestStart : IStartUpAction
    //{
    //    private sealed class TestStop : SupervisorStrategy
    //    {
    //        protected override Directive Handle(IActorRef child, Exception exception)
    //        {
    //            return Directive.Stop;
    //        }

    //        public override void ProcessFailure(IActorContext context, bool restart, IActorRef child, Exception cause, ChildRestartStats stats, IReadOnlyCollection<ChildRestartStats> children)
    //        {

    //        }

    //        public override void HandleChildTerminated(IActorContext actorContext, IActorRef child, IEnumerable<IInternalActorRef> children)
    //        {
    //        }

    //        public override ISurrogate ToSurrogate(ActorSystem system) => null!;

    //        public override IDecider Decider => DefaultDecider;
    //    }

    //    private sealed class Watcher : ReceiveActor
    //    {
    //        public Watcher()
    //        {
    //            ReceiveAsync<Start>(Start);
    //        }

    //        protected override SupervisorStrategy SupervisorStrategy() => new TestStop();

    //        private async Task Start(Start obj)
    //        {
    //            try
    //            {
    //                var target = DataTransferManager.New(Context, "Target");
    //                var api = TestApi.Get(Context);

    //                for (int i = 0; i < 10; i++)
    //                {
    //                    try
    //                    {
    //                        Log.Information($"Start Test {i}");

    //                        var res = await api.Send(new TestCommand(), TimeSpan.FromMinutes(1), target, Log.Information, () => File.OpenWrite("Program.txt"));

    //                        //Thread.Sleep(1000);

    //                        switch (res)
    //                        {
    //                            case TransferFailed f:
    //                                Log.Information($"Test Failed: {i} {f.Reason}");
    //                                break;
    //                            case TransferSucess:
    //                                Log.Information($"Test Sucess: {i}");
    //                                break;
    //                        }
    //                    }
    //                    catch (Exception e)
    //                    {
    //                        Log.Information($"Transfer Failed {i}");
    //                        Console.WriteLine(e);
    //                    }
    //                }
    //            }
    //            finally
    //            {
    //                await Context.System.Terminate();
    //            }
    //        }
    //    }

    //    private readonly ActorSystem _testSytem;

    //    public TestStart(ActorSystem testSytem) => _testSytem = testSytem;

    //    public void Run()
    //    {
    //        _testSytem.ActorOf<Watcher>("Watcher").Tell(new Start());
    //    }
    //}

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
                                  .ConfigureAutoFac(cb =>
                                                    {
                                                        cb.RegisterType<ConsoleAppRoute>().Named<IAppRoute>("default");
                                                        cb.RegisterType<TestStart>().As<IStartUpAction>();
                                                    })
                                  .Build().Run();
        }
    }
}