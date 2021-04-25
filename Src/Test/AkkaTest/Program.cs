using System;
using System.IO;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaTest.InMemoryStorage;
using ServiceManager.ProjectRepository;
using SharpRepository.InMemoryRepository;
using SharpRepository.Repository.Configuration;
using Tauron.Application.AkkaNode.Services.CleanUp;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.Files.VirtualFiles;
using Tauron.Features;
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

    public sealed class TestActor : ReceiveActor
    {
        private readonly RepositoryManager _repositoryManager;
        private readonly DataTransferManager _dataTransfer;

        public TestActor(RepositoryManager repositoryManager, DataTransferManager dataTransfer)
        {
            _repositoryManager = repositoryManager;
            _dataTransfer = dataTransfer;
            Receive<Start>(Start);
        }

        private void Start(Start obj)
        {
            
        }
    }

    internal static class Program
    {

        private static async Task Main()
        {
            //const string con = "mongodb://192.168.105.96:27017/TestDb?readPreference=primary&appname=MongoDB%20Compass%20Community&ssl=false";
            ////const string con = "mongodb://localhost:27017/TestDb?readPreference=primary&appname=MongoDB%20Compass%20Community&ssl=false";

            var factory = new VirtualFileFactory();
            var bucked = factory.CreateInMemory("Test");


            using var system = ActorSystem.Create("Test");
            string tempPath = Path.GetFullPath("Temp");
            using var tempStore = TempStorage.CleanAndCreate(tempPath);
            var config = new SharpRepositoryConfiguration();

            //config.AddRepository(new MongoDbRepositoryConfiguration(CleanUpManager.RepositoryKey, con){ Factory = typeof(MongoDbConfigRepositoryFactory) });
            config.AddRepository(new InMemoryRepositoryConfiguration(RepositoryManager.RepositoryManagerKey) { Factory = typeof(PersistentInMemorxConfigRepositoryFactory) });

            const string testFileName = "Program.cs";

            var dataManager = DataTransferManager.New(system);
            var manager = RepositoryManager.CreateInstance(system, new RepositoryManagerConfiguration(config, bucked, dataManager));

            system.ActorOf(Props.Create(() => new TestActor(manager, dataManager))).Tell(new Start());

            Console.ReadKey();

            await system.Terminate();
        }
    }
}