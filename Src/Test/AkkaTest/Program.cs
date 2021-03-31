using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Actor.Setup;
using Akka.Configuration;
using Akka.DependencyInjection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using SharpRepository.InMemoryRepository;
using SharpRepository.MongoDbRepository;
using SharpRepository.Repository;
using SharpRepository.Repository.Configuration;

namespace AkkaTest
{
    public sealed class HalloWeltActor : ReceiveActor
    {
        public HalloWeltActor(ILifetimeScope scope) => Receive<string>(c => Console.WriteLine($"Hallo: {c}"));
    }

    public sealed record MyClass(string Id, string Value)
    {
        public MyClass()
            : this("", "") { }
    }

    internal static class Program
    {

        private static async Task Main()
        {
            const string con = "mongodb://192.168.105.96:27017/?readPreference=primary&appname=MongoDB%20Compass%20Community&ssl=false";

            SharpRepositoryConfiguration config = new();

            var con2 = new MongoUrlBuilder();
            con2.Parse(con);
            con2.ApplicationName = "Test";
            con2.DatabaseName = "Test2";

            var test = new MongoDbRepository<MyClass, string>(con2.ToString());
            test.Add(new MyClass("1", "Hallo"));

            var t = test.Get("1");

            //var containerBuilder = new ContainerBuilder();
            //containerBuilder.RegisterType<HalloWeltActor>();

            //await using var serviceProvider = new AutofacServiceProvider(containerBuilder.Build());
            //var setup = ActorSystemSetup.Empty
            //                            .WithSetup(ServiceProviderSetup.Create(serviceProvider))
            //                            .WithSetup(BootstrapSetup.Create().WithConfig(Config.Empty));

            //var system = ActorSystem.Create("TestSystem", setup);
            //system.RegisterExtension(new ServiceProviderExtension());

            //var sp = system.GetExtension<ServiceProvider>();
            //var props = sp.Props<HalloWeltActor>();

            //var actor = system.ActorOf(props, "Hallo");

            //while (true)
            //{
            //    var test = Console.ReadLine();

            //    if(string.IsNullOrWhiteSpace(test))
            //        break;

            //    actor.Tell(test);
            //}

            //await system.Terminate();
        }
    }
}