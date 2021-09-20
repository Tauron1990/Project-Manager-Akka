using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
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
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Application.VirtualFiles;
using Tauron.Features;
using Tauron.ObservableExt;

namespace ServiceManager.Server.AppCore.ServiceDeamon
{
    public sealed record TryStart(string? DatabaseUrl);

    public sealed record TryStartResponse(bool IsRunning, string Message);

    public interface IProcessServiceHost : IFeatureActorRef<IProcessServiceHost>
    {
        public Task<TryStartResponse> TryStart(string? databaseUrl);

        public Task<string> ConfigAlive(ConfigurationApi api, ActorSystem system);
    }

    [UsedImplicitly]
    public sealed class ProcessServiceHostRef : FeatureActorRefBase<IProcessServiceHost>, IProcessServiceHost
    {
        public ProcessServiceHostRef() 
            : base("ServiceDeamonHost")
        {
        }

        public Task<TryStartResponse> TryStart(string? databaseUrl) => Ask<TryStartResponse>(new TryStart(databaseUrl), TimeSpan.FromMinutes(1));

        public async Task<string> ConfigAlive(ConfigurationApi api, ActorSystem system)
        {
            var startResponse = await TryStart(null);
            if (!startResponse.IsRunning)
                return startResponse.Message;
            var isAlive = await api.QueryIsAlive(system, TimeSpan.FromSeconds(10));
            return isAlive.IsAlive ? string.Empty : "Der Api Service ist nicht Verfügbar";
        }
    }

    public sealed class ProcessServiceHostActor : ActorFeatureBase<ProcessServiceHostActor.State>
    {
        public sealed record State(IDatabaseConfig DatabaseConfig, Services Operator, IHostEnvironment HostEnvironment, RepositoryApi RepositoryApi, DeploymentApi DeploymentApi, ConfigurationApi ConfigurationApi);

        public static Func<IHostEnvironment, IDatabaseConfig, RepositoryApi, DeploymentApi, ConfigurationApi, IPreparedFeature> New()
        {
            static IPreparedFeature _(IHostEnvironment hostEnvironment, IDatabaseConfig config, RepositoryApi repositoryApi, DeploymentApi deploymentApi, ConfigurationApi configurationApi)
                => Feature.Create(() => new ProcessServiceHostActor(), _ => new State(config, new Services(), hostEnvironment, repositoryApi, deploymentApi, configurationApi));

            return _;
        }

        protected override void ConfigImpl()
        {
            Receive<TryStart>(obs => obs.Synchronize()
                                    .ConditionalSelect()
                                        .ToResult<StatePair<TryStartResponse, State>>(
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
                                                                  ? Task.FromResult(pair.NewEvent(new TryStartResponse(IsRunning: true, string.Empty)))
                                                                  : TryStart(pair)
                                                              select response,
                                                         (pair, exception) =>
                                                         {
                                                             Log.Error(exception, "Error on start in Process ServiceManager Deamon");
                                                             return Observable.Return(pair.NewEvent(new TryStartResponse(IsRunning: false, Message: exception.Message)));
                                                         }));
                                             })
                                        .ToUnit(p => p.Sender.Tell(p.Event)));

            Receive<Services>(obs => obs.Do(p =>
                                            {
                                                var (evt, _) = p;

                                                if(!evt.Running) return;

                                                Context.Watch(evt.Deploy);
                                                Context.Watch(evt.Deamon);
                                                Context.Watch(evt.Repo);
                                            }).Select(p => p.State with {Operator = p.State.Operator.MakeNew(p.Event)}));

            Receive<Terminated>(obs => obs.Select(_ => new Services()).ToSelf());
        }

        
        private async Task<StatePair<TryStartResponse, State>> TryStart(StatePair<TryStart, State> o)
        {
            using var gate = await StartSyncronizer.Wait();

            if (o.State.Operator.Running)
                return o.NewEvent(new TryStartResponse(IsRunning: true, string.Empty));

            if (gate.Success)
            {
                gate.SetSuccess(value: false);
                return o.NewEvent(new TryStartResponse(IsRunning: true, string.Empty));
            }
                
            gate.SetSuccess(value: false);
                
            var isError = false;
            IActorRef repo = ActorRefs.Nobody;
            IActorRef deploy = ActorRefs.Nobody;
            IActorRef deamon = ActorRefs.Nobody;

            string databseUrl = await o.State.DatabaseConfig.GetUrl();

            try
            {
                if (string.IsNullOrWhiteSpace(o.Event.DatabaseUrl))
                {
                    if (!await o.State.DatabaseConfig.GetIsReady())
                        return o.NewEvent(new TryStartResponse(IsRunning: false, "Keine Datenbaank Url Verfügbar"));
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

            o.Self.Tell(new Services(repo, deploy, deamon, Running: true, databseUrl));
            gate.SetSuccess();
            return o.NewEvent(new TryStartResponse(IsRunning: true, string.Empty));
        }

        private static void StartServices(string connectionstring, IHostEnvironment hostEnvironment, ActorSystem system,
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

            #pragma warning disable GU0011
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
            #pragma warning disable GU0011
        }

        public sealed record Services(IActorRef Repo, IActorRef Deploy, IActorRef Deamon, bool Running, string CurrentUrl) : IDisposable
        {
            public Services()
                : this(ActorRefs.Nobody, ActorRefs.Nobody, ActorRefs.Nobody, Running: false, string.Empty)
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
        
        public static class StartSyncronizer
        {
            private static readonly SemaphoreSlim MainGate = new(1);
            private static bool _success;

            public static async Task<SyncGate> Wait()
            {
                await MainGate.WaitAsync();

                return new SyncGate(MainGate, _success);
            }

            public sealed class SyncGate : IDisposable
            {
                private readonly SemaphoreSlim _gate;

                public bool Success { get; }
                
                public SyncGate(SemaphoreSlim gate, bool success)
                {
                    _gate = gate;
                    Success = success;
                }

                public void SetSuccess(bool value = true)
                    => _success = value;
                
                public void Dispose()
                    => _gate.Release();
            }
        }
    }
}