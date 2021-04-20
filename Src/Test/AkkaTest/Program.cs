using System;
using System.IO;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaTest.InMemoryStorage;
using MongoDB.Driver;
using SharpRepository.InMemoryRepository;
using SharpRepository.MongoDbRepository;
using SharpRepository.Repository.Configuration;
using Tauron.Application.AkkaNode.Services.CleanUp;
using Tauron.Features;
using Tauron.Temp;
using YellowDrawer.Storage.Common.FileSystem;

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

    internal static class Program
    {

        private static async Task Main()
        {
            //const string con = "mongodb://192.168.105.96:27017/TestDb?readPreference=primary&appname=MongoDB%20Compass%20Community&ssl=false";
            //const string con = "mongodb://localhost:27017/TestDb?readPreference=primary&appname=MongoDB%20Compass%20Community&ssl=false";


            using var system = ActorSystem.Create("Test");
            string tempPath = Path.GetFullPath("Temp");
            using var tempStore = TempStorage.CleanAndCreate(tempPath);

            var storage = new FileSystemStorageProvider(tempPath);
            var config = new SharpRepositoryConfiguration();

            //config.AddRepository(new MongoDbRepositoryConfiguration(CleanUpManager.RepositoryKey, con){ Factory = typeof(MongoDbConfigRepositoryFactory) });
            config.AddRepository(new InMemoryRepositoryConfiguration(CleanUpManager.RepositoryKey){ Factory = typeof(PersistentInMemorxConfigRepositoryFactory)});

            const string testFileName = "Program.cs";

            config.GetInstance<CleanUpTime, string>(CleanUpManager.RepositoryKey).Add(new CleanUpTime("Master", TimeSpan.FromDays(7), DateTime.Now - TimeSpan.FromDays(10)));
            config.GetInstance<ToDeleteRevision, string>(CleanUpManager.RepositoryKey).Add(new ToDeleteRevision(testFileName, testFileName));

            storage.CreateFile(testFileName, await File.ReadAllBytesAsync(testFileName));

            var manager = system.ActorOf(CleanUpManager.New(config, storage));
            manager.Tell(CleanUpManager.Initialization);

            Console.ReadKey();

            await system.Terminate();
        }
    }
}