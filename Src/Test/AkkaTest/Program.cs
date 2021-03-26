using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Actor.Setup;
using Akka.Configuration;
using Akka.DependencyInjection;
using Autofac;
using Autofac.Extensions.DependencyInjection;

namespace AkkaTest
{
    public sealed class HalloWeltActor : ReceiveActor
    {
        public HalloWeltActor(ILifetimeScope scope) => Receive<string>(c => Console.WriteLine($"Hallo: {c}"));
    }

    internal static class Program
    {

        private static async Task Main()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<HalloWeltActor>();

            await using var serviceProvider = new AutofacServiceProvider(containerBuilder.Build());
            var setup = ActorSystemSetup.Empty
                                        .WithSetup(ServiceProviderSetup.Create(serviceProvider))
                                        .WithSetup(BootstrapSetup.Create().WithConfig(Config.Empty));

            var system = ActorSystem.Create("TestSystem", setup);
            system.RegisterExtension(new ServiceProviderExtension());

            var sp = system.GetExtension<ServiceProvider>();
            var props = sp.Props<HalloWeltActor>();

            var actor = system.ActorOf(props, "Hallo");

            while (true)
            {
                var test = Console.ReadLine();

                if(string.IsNullOrWhiteSpace(test))
                    break;

                actor.Tell(test);
            }

            await system.Terminate();
        }
    }
}