using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using NLog;
using ServiceHost.ClientApp.Shared;
using ServiceHost.ClientApp.Shared.ConfigurationServer.Data;
using ServiceManager.ProjectDeployment;
using ServiceManager.ProjectDeployment.Data;
using ServiceManager.ProjectRepository;
using ServiceManager.ServiceDeamon.Management;
using SharpRepository.MongoDbRepository;
using SharpRepository.Repository.Configuration;
using Tauron;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Services.CleanUp;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.Files.GridFS;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Application.Master.Commands.KillSwitch;
using Tauron.Application.Master.Commands.ServiceRegistry;
using Tauron.Application.MongoExtensions;
using Tauron.Application.VirtualFiles;

namespace InfrastructureService
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            ImmutableListSerializer<Condition>.Register();
            ImmutableListSerializer<SeedUrl>.Register();
            ImmutableListSerializer<AppFileInfo>.Register();

            await AppNode.StartNode(
                    args,
                    KillRecpientType.Service,
                    IpcApplicationType.Client,
                    ab =>
                    {
                        ab.OnMemberUp(
                                (context, system, cluster) =>
                                {
                                    MongoUrl ApplyMongoUrl(string baseUrl, string repoKey, SharpRepositoryConfiguration configuration)
                                    {
                                        var builder = new MongoUrlBuilder(baseUrl) { DatabaseName = repoKey, ApplicationName = context.HostingEnvironment.ApplicationName };
                                        var mongoUrl = builder.ToString();

                                        configuration.AddRepository(
                                            new MongoDbRepositoryConfiguration(repoKey, mongoUrl)
                                            {
                                                Factory = typeof(MongoDbConfigRepositoryFactory),
                                            });

                                        return new MongoUrl(mongoUrl);
                                    }

                                    ServiceRegistryApi.Get(system)
                                       .RegisterService(
                                            new RegisterService(
                                                ServiceName.From(context.HostingEnvironment.ApplicationName),
                                                cluster.SelfUniqueAddress,
                                                ServiceTypes.Infrastructure));

                                    var connectionstring = system.Settings.Config.GetString("akka.persistence.snapshot-store.mongodb.connection-string");
                                    if (string.IsNullOrWhiteSpace(connectionstring))
                                    {
                                        LogManager.GetCurrentClassLogger().Error("No Mongo DB Connection provided: Shutdown");
                                        system.Terminate().Ignore();

                                        return;
                                    }

                                    var config = new SharpRepositoryConfiguration();
                                    var fileSystemBuilder = new VirtualFileFactory();

                                    #pragma warning disable GU0011

                                    ApplyMongoUrl(connectionstring, CleanUpManager.RepositoryKey, config);

                                    var url = ApplyMongoUrl(connectionstring, RepositoryManager.RepositoryKey, config);
                                    RepositoryManager.InitRepositoryManager(
                                        system,
                                        new RepositoryManagerConfiguration(
                                            config,
                                            fileSystemBuilder.GridFs(url),
                                            DataTransferManager.New(system, "Repository-DataTransfer")));

                                    url = ApplyMongoUrl(connectionstring, DeploymentManager.RepositoryKey, config);
                                    DeploymentManager.InitDeploymentManager(
                                        system,
                                        new DeploymentConfiguration(
                                            config,
                                            fileSystemBuilder.GridFs(url),
                                            DataTransferManager.New(system, "Deployment-DataTransfer"),
                                            RepositoryApi.CreateProxy(system)));

                                    ApplyMongoUrl(connectionstring, ServiceManagerDeamon.RepositoryKey, config);
                                    ServiceManagerDeamon.Init(system, config);
                                    #pragma warning restore GU0011
                                })
                           .OnMemberRemoved((_, system, _) => system.Terminate());
                    },
                    consoleLog: true)
               .Build().RunAsync();
        }
    }
}