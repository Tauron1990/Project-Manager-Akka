﻿using Akka.Actor;
using ServiceHost.ClientApp.Shared.ConfigurationServer;
using ServiceHost.ClientApp.Shared.ConfigurationServer.Events;
using ServiceManager.Server.AppCore;
using ServiceManager.Server.AppCore.Apps;
using ServiceManager.Server.AppCore.ClusterTracking;
using ServiceManager.Server.AppCore.ClusterTracking.Data;
using ServiceManager.Server.AppCore.ServiceDeamon;
using ServiceManager.Server.AppCore.Settings;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Data;
using Tauron.Application.Master.Commands.Deployment.Repository;

namespace ServiceManager.Server
{
    public sealed class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterSettingsManager(c => c.WithProvider<LocalConfigurationProvider>());
            builder.RegisterType<LocalConfiguration>().As<ILocalConfiguration>().SingleInstance();

            builder.RegisterEventDispatcher<IConfigEvent, ConfigEventDispatcher, ConfigApiEventDispatcherRef>(ConfigApiEventDispatcherActor.New(), "ConfigApiEventDispatcherActor");
            builder.RegisterEventDispatcher<AppInfo, AppEventDispatcher, AppEventDispatcherRef>(AppEventDispatcherActor.New(), "AppEventDispatcherRef");

            builder.RegisterType<NodeRepository>().As<INodeRepository>().SingleInstance();
            builder.RegisterType<PropertyChangedNotifer>().As<IPropertyChangedNotifer>().SingleInstance();

            builder.RegisterFeature<ClusterNodeManagerRef, IClusterNodeManager>(ClusterHostManagerActor.New(), "ClusterHostManagerActor");
            builder.RegisterFeature<ProcessServiceHostRef, IProcessServiceHost>(ProcessServiceHostActor.New());

            builder.Register(c => DeploymentApi.CreateProxy(c.Resolve<ActorSystem>())).SingleInstance();
            builder.Register(c => RepositoryApi.CreateProxy(c.Resolve<ActorSystem>())).SingleInstance();
            builder.Register(c => ConfigurationApi.CreateProxy(c.Resolve<ActorSystem>())).SingleInstance();

            builder.RegisterStartUpAction<ActorStartUp>();

            base.Load(builder);
        }
    }
}