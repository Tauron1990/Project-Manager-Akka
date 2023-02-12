using System.Threading.Tasks;
using Akka.Cluster;
using DotNetty.Transport.Bootstrapping;
using Master.Seed.Node.Commands;
using Microsoft.Extensions.Hosting;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Host;
using ServiceHost.ClientApp.Shared;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.Master.Commands.KillSwitch;
using Tauron.Application.Master.Commands.ServiceRegistry;

namespace Master.Seed.Node
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            //Beacon? beacon = null;

            await AppNode.StartNode(
                    args,
                    KillRecpientType.Seed,
                    IpcApplicationType.Client,
                    ab => ab.ConfigureAkka(
                        (context, systemBuilder) =>
                        {
                            systemBuilder.AddStartup(
                                (system, _) =>
                                {
                                    var cluster = Cluster.Get(system);
                                    cluster.RegisterOnMemberUp(
                                        () =>
                                        {
                                            ServiceRegistryApi.Start(
                                                system,
                                                new RegisterService(
                                                    ServiceName.From(context.HostingEnvironment.ApplicationName),
                                                    cluster.SelfUniqueAddress,
                                                    ServiceTypes.SeedNode));
                                        });

                                    var cmd = PetabridgeCmd.Get(system);
                                    cmd.RegisterCommandPalette(ClusterCommands.Instance);
                                    cmd.RegisterCommandPalette(MasterCommand.New);
                                    cmd.Start();
                                });
                        }))
               .Build().RunAsync();
        }
    }
}