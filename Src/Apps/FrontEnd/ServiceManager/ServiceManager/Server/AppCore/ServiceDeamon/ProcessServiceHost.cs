using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using MongoDB.Driver;
using ServiceHost.Client.Shared.ConfigurationServer;
using ServiceManager.ProjectDeployment;
using ServiceManager.ProjectRepository;
using ServiceManager.ServiceDeamon.Management;
using ServiceManager.Shared.ServiceDeamon;
using SharpRepository.MongoDbRepository;
using SharpRepository.Repository.Configuration;
using Tauron;
using Tauron.Application.AkkaNode.Services.CleanUp;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.Files.GridFS;
using Tauron.Application.Files.VirtualFiles;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Features;
using Tauron.Host;
using Tauron.ObservableExt;

namespace ServiceManager.Server.AppCore.ServiceDeamon
{
    public sealed record TryStart(string? DatabaseUrl);

    public sealed record TryStartResponse(bool IsRunning, string Message);

    public interface IProcessServiceHost : IFeatureActorRef<IProcessServiceHost>
    {
        public Task<TryStartResponse> TryStart(string? databaseUrl);
    }

    public sealed class ProcessServiceHostRef : FeatureActorRefBase<IProcessServiceHost>, IProcessServiceHost
    {
        public ProcessServiceHostRef() 
            : base("ServiceDeamonHost")
        {
        }

        public Task<TryStartResponse> TryStart(string? databaseUrl) => Ask<TryStartResponse>(new TryStart(databaseUrl), TimeSpan.FromMinutes(1));
    }

    public sealed class ProcessServiceHostActor : ActorFeatureBase<ProcessServiceHostActor._>
    {
        public sealed record _(IDatabaseConfig DatabaseConfig, Services Operator, IActorHostEnvironment HostEnvironment, RepositoryApi RepositoryApi, DeploymentApi DeploymentApi, ConfigurationApi ConfigurationApi);

        public static Func<IActorHostEnvironment, IDatabaseConfig, RepositoryApi, DeploymentApi, ConfigurationApi, IPreparedFeature> New()
        {
            static IPreparedFeature _(IActorHostEnvironment hostEnvironment, IDatabaseConfig config, RepositoryApi repositoryApi, DeploymentApi deploymentApi, ConfigurationApi configurationApi)
                => Feature.Create(() => new ProcessServiceHostActor(), _ => new _(config, new Services(), hostEnvironment, repositoryApi, deploymentApi, configurationApi));

            return _;
        }

        protected override void ConfigImpl()
        {
            Receive<TryStart>(obs => obs.ConditionalSelect()
                                        .ToResult<StatePair<TryStartResponse, _>>(
                                             b =>
                                             {
                                                 b.When(p => !p.State.Operator.Running, o => from pair in o
                                                                                             from result in TryStart(pair)
                                                                                             select result);

                                                 b.When(p => p.State.Operator.Running && p.State.Operator.CurrentUrl != p.Event.DatabaseUrl,
                                                     o => from pair in o
                                                          from result in TryStart(pair)
                                                          select result);

                                                 b.When(p => p.State.Operator.Running && p.State.Operator.CurrentUrl == p.Event.DatabaseUrl,
                                                     o => o.CatchSafe(
                                                         p => from pair in Observable.Return(p)
                                                              let system = pair.Context.System
                                                              let timeout = TimeSpan.FromSeconds(20)
                                                              from repoResult in pair.State.RepositoryApi.QueryIsAlive(system, timeout)
                                                              from deployResult in pair.State.DeploymentApi.QueryIsAlive(system, timeout)
                                                              from configResult in pair.State.ConfigurationApi.QueryIsAlive(system, timeout)
                                                              let isOk = repoResult.IsAlive && deployResult.IsAlive && configResult.IsAlive
                                                              from response in isOk
                                                                  ? Observable.Return(pair.NewEvent(new TryStartResponse(true, string.Empty)))
                                                                  : TryStart(pair)
                                                              select response,
                                                         (pair, exception) =>
                                                         {
                                                             Log.Error(exception, "Error on start in Process ServiceManager Deamon");
                                                             return Observable.Return(pair.NewEvent(new TryStartResponse(false, exception.Message)));
                                                         }));
                                             })
                                        .ToUnit(p => p.Sender.Tell(p.Event)));

            Receive<Services>(obs => obs.Do(p =>
                                            {
                                                if(!p.Event.Running) return;

                                                Context.Watch(p.Event.Deploy);
                                                Context.Watch(p.Event.Deamon);
                                                Context.Watch(p.Event.Repo);
                                            }).Select(p => p.State with {Operator = p.State.Operator.MakeNew(p.Event)}));

            Receive<Terminated>(obs => obs.Select(_ => new Services()).ToSelf());
        }

        private static IObservable<StatePair<TryStartResponse, _>> TryStart(StatePair<TryStart, _> o)
        {
            if (o.State.Operator.Running)
                return Observable.Return(o.NewEvent(new TryStartResponse(true, string.Empty)));

            var isError = false;
            IActorRef repo = ActorRefs.Nobody;
            IActorRef deploy = ActorRefs.Nobody;
            IActorRef deamon = ActorRefs.Nobody;

            string databseUrl = o.State.DatabaseConfig.Url;

            try
            {
                if (string.IsNullOrWhiteSpace(o.Event.DatabaseUrl))
                {
                    if (!o.State.DatabaseConfig.IsReady)
                        return Observable.Return(o.NewEvent(new TryStartResponse(false, "Keine Datenbaank Url Verfügbar")));
                }
                else
                    databseUrl = o.Event.DatabaseUrl;

                StartServices(databseUrl, o.State.HostEnvironment, o.Context.System, out repo, out deploy, out deamon);
            }
            catch
            {
                isError = true;
                throw;
            }
            finally
            {
                if (isError)
                {
                    repo.Tell(PoisonPill.Instance);
                    deploy.Tell(PoisonPill.Instance);
                    deamon.Tell(PoisonPill.Instance);
                }
            }

            o.Self.Tell(new Services(repo, deploy, deamon, true, databseUrl));
            return Observable.Return(o.NewEvent(new TryStartResponse(true, string.Empty)));
        }

        private static void StartServices(string connectionstring, IActorHostEnvironment hostEnvironment, ActorSystem system,
            out IActorRef repoRef, out IActorRef deployRef, out IActorRef deamonRef)
        {
            string ApplyMongoUrl(string baseUrl, string repoKey, SharpRepositoryConfiguration configuration)
            {
                var builder = new MongoUrlBuilder(baseUrl) {DatabaseName = repoKey, ApplicationName = hostEnvironment.ApplicationName};
                var mongoUrl = builder.ToString();

                configuration.AddRepository(new MongoDbRepositoryConfiguration(repoKey, mongoUrl)
                                            {
                                                Factory = typeof(MongoDbConfigRepositoryFactory)
                                            });

                return mongoUrl;
            }

            var config = new SharpRepositoryConfiguration();
            var fileSystemBuilder = new VirtualFileFactory();

            ApplyMongoUrl(connectionstring, CleanUpManager.RepositoryKey, config);

            var url = ApplyMongoUrl(connectionstring, RepositoryManager.RepositoryKey, config);
            repoRef =
                RepositoryManager.InitRepositoryManager(system,
                                      new RepositoryManagerConfiguration(config, fileSystemBuilder.CreateMongoDb(url),
                                          DataTransferManager.New(system, "Repository-DataTransfer")))
                                 .Manager;

            url = ApplyMongoUrl(connectionstring, DeploymentManager.RepositoryKey, config);
            deployRef =
                DeploymentManager.InitDeploymentManager(system,
                                      new DeploymentConfiguration(config,
                                          fileSystemBuilder.CreateMongoDb(url),
                                          DataTransferManager.New(system, "Deployment-DataTransfer"),
                                          RepositoryApi.CreateProxy(system, "Deployment-Repository-Proxy")))
                                 .Manager;

            ApplyMongoUrl(connectionstring, ServiceManagerDeamon.RepositoryKey, config);
            deamonRef = ServiceManagerDeamon.Init(system, config);
        }

        public sealed record Services(IActorRef Repo, IActorRef Deploy, IActorRef Deamon, bool Running, string CurrentUrl) : IDisposable
        {
            public Services()
                : this(ActorRefs.Nobody, ActorRefs.Nobody, ActorRefs.Nobody, false, string.Empty)
            {
                
            }

            public void Dispose()
            {
                Repo.Tell(PoisonPill.Instance);
                Deploy.Tell(PoisonPill.Instance);
                Deamon.Tell(PoisonPill.Instance);
            }

            public Services MakeNew(Services services)
            {
                Dispose();
                return services;
            }
        }
    }
}