﻿using System;
using System.Threading.Tasks;
using System.Windows.Data;
using Akka.Actor;
using Akka.Configuration;
using Akka.DI.Core;
using ServiceManager.ProjectDeployment;
using ServiceManager.ProjectRepository;
using Tauron.Akka;
using Tauron.Application.AkkNode.Services.FileTransfer;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Application.ServiceManager.Core.Configuration;
using Tauron.Application.ServiceManager.Core.Model.Event;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.Core.Model
{
    public sealed class DeploymentServices
    {
        private readonly IActorRef _manager;

        public DeploymentServices(ActorSystem system) => _manager = system.ActorOf(system.DI().Props<ServiceManager>(), "Deployment_Service_Manager");

        public Task<DeploymentServicesChanged> GetCurrent()
            => _manager.Ask<DeploymentServicesChanged>(new GetCurrentState());

        public void PushNewConnectionString(string url)
            => _manager.Tell(new NewConnectionString(url));

        private sealed class Init { }

        private sealed class NewConnectionString
        {
            public string Connection { get; }

            public NewConnectionString(string connection) => Connection = connection;
        }

        private sealed class GetCurrentState
        {
            
        }

        private sealed class ServiceManager : ExposedReceiveActor
        {
            private readonly AppConfig _config;
            private readonly IActorRef _dataTransfer;
            private readonly RepositoryApi _repositoryApi;

            private RepositoryManager? _repository;
            private DeploymentManager? _deploymentServer;

            public ServiceManager(AppConfig config)
            {
                _config = config;
                _dataTransfer = DataTransferManager.New(Context, "File_Coordination");
                _repositoryApi = RepositoryApi.CreateProxy(Context.System);

                Receive<Init>(_ =>
                {
                    try
                    {
                        if(string.IsNullOrWhiteSpace(config.CurrentConfig))
                            return;

                        var hConfig = ConfigurationFactory.ParseString(config.CurrentConfig);
                        var connectionString = hConfig.GetString("akka.persistence.journal.mongodb.connection-string");

                        if(string.IsNullOrWhiteSpace(connectionString))
                            return;

                        InitClient(connectionString);
                    }
                    catch (Exception e)
                    {
                        Log.Warning(e, "Error on Parsing Current Configuration");
                    }

                    Context.System.EventStream.Publish(new DeploymentServicesChanged(_repository?.IsOk == true && _deploymentServer?.IsOk == true));
                });
                Receive<NewConnectionString>(s =>
                {
                    try
                    {
                        InitClient(s.Connection);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error on Init new Connection String");
                    }

                    Context.System.EventStream.Publish(new DeploymentServicesChanged(_repository?.IsOk == true && _deploymentServer?.IsOk == true));
                });
                Receive<GetCurrentState>(_ => Sender.Tell(new DeploymentServicesChanged(_repository?.IsOk == true && _deploymentServer?.IsOk == true)));
            }

            private void InitClient(string connection)
            {
                _repository?.Stop();
                _repository = null;

                _deploymentServer?.Stop();
                _deploymentServer = null;

                _repository = RepositoryManager.InitRepositoryManager(Context.System, connection, _dataTransfer);
                _deploymentServer = DeploymentManager.InitDeploymentManager(Context.System, connection, _dataTransfer, _repositoryApi);
            }

            protected override void PreStart()
            {
                Self.Tell(new Init());
                base.PreStart();
            }
        }
    }
}