using System.Threading.Tasks;
using Akka.Cluster;
using Master.Seed.Node.Commands;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Host;
using ServiceHost.Client.Shared;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.Master.Commands.KillSwitch;
using Tauron.Application.Master.Commands.ServiceRegistry;

namespace Master.Seed.Node
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            //Beacon? beacon = null;

            await Bootstrap.StartNode(args, KillRecpientType.Seed, IpcApplicationType.Client)
                .ConfigurateAkkaSystem((context, system) =>
                {
                    var cluster = Cluster.Get(system);
                    cluster.RegisterOnMemberUp(() =>
                    {
                        ServiceRegistry.Start(system,
                            new RegisterService(
                                context.HostEnvironment.ApplicationName,
                                cluster.SelfUniqueAddress,
                                ServiceTypes.SeedNode));
                    });

                    var cmd = PetabridgeCmd.Get(system);
                    cmd.RegisterCommandPalette(ClusterCommands.Instance);
                    cmd.RegisterCommandPalette(MasterCommand.New);
                    cmd.Start();
                })
                .Build().Run();
        }
    }
}