using System.Threading.Tasks;
using MongoDB.Driver;
using NLog;
using ServiceHost.Client.Shared;
using ServiceManager.ProjectDeployment;
using ServiceManager.ProjectRepository;
using ServiceManager.ServiceDeamon.Management;
using SharpRepository.MongoDbRepository;
using SharpRepository.Repository.Configuration;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.Files.GridFS;
using Tauron.Application.Files.VirtualFiles;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Application.Master.Commands.KillSwitch;
using Tauron.Application.Master.Commands.ServiceRegistry;

namespace InfrastructureService
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await Bootstrap.StartNode(args, KillRecpientType.Service, IpcApplicationType.Client, true)
                           .OnMemberUp((context, system, cluster) =>
                                                  {
                                                      string ApplyMongoUrl(string baseUrl, string repoKey, SharpRepositoryConfiguration configuration)
                                                      {
                                                          var builder = new MongoUrlBuilder(baseUrl) {DatabaseName = repoKey, ApplicationName = context.HostEnvironment.ApplicationName};
                                                          var mongoUrl = builder.ToString();

                                                          configuration.AddRepository(new MongoDbRepositoryConfiguration(repoKey, mongoUrl)
                                                                                      {
                                                                                          Factory = typeof(MongoDbConfigRepositoryFactory)
                                                                                      });

                                                          return mongoUrl;
                                                      }

                                                      ServiceRegistry.Get(system)
                                                                     .RegisterService(new RegisterService(
                                                                          context.HostEnvironment.ApplicationName, 
                                                                          cluster.SelfUniqueAddress,
                                                                          ServiceTypes.Infrastructure));

                                                      var connectionstring = system.Settings.Config.GetString("akka.persistence.snapshot-store.mongodb.connection-string");
                                                      if (string.IsNullOrWhiteSpace(connectionstring))
                                                      {
                                                          LogManager.GetCurrentClassLogger().Error("No Mongo DB Connection provided: Shutdown");
                                                          system.Terminate();
                                                          return;
                                                      }

                                                      var config = new SharpRepositoryConfiguration();
                                                      var fileSystemBuilder = new VirtualFileFactory();

                                                      var url = ApplyMongoUrl(connectionstring, RepositoryManager.RepositoryKey, config);
                                                      RepositoryManager.InitRepositoryManager(system,
                                                          new RepositoryManagerConfiguration(config, fileSystemBuilder.CreateMongoDb(url),
                                                              DataTransferManager.New(system, "Repository-DataTransfer")));

                                                      url = ApplyMongoUrl(connectionstring, DeploymentManager.RepositoryKey, config);
                                                      DeploymentManager.InitDeploymentManager(system,
                                                          new DeploymentConfiguration(config, 
                                                              fileSystemBuilder.CreateMongoDb(url),
                                                              DataTransferManager.New(system, "Deployment-DataTransfer"), 
                                                              RepositoryApi.CreateProxy(system)));

                                                      ApplyMongoUrl(connectionstring, ServiceManagerDeamon.RepositoryKey, config);
                                                      ServiceManagerDeamon.Init(system, config);
                                                  })
                           .OnMemberRemoved((_, system, _) => system.Terminate())
                           .Build().Run();
        }
    }
}
