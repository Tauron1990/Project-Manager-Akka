using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using ServiceHost.ClientApp.Shared.ConfigurationServer;
using ServiceHost.ClientApp.Shared.ConfigurationServer.Events;
using ServiceManager.Server.AppCore;
using ServiceManager.Server.AppCore.Apps;
using ServiceManager.Server.AppCore.ClusterTracking;
using ServiceManager.Server.AppCore.ClusterTracking.Data;
using ServiceManager.Server.AppCore.ServiceDeamon;
using ServiceManager.Server.AppCore.Settings;
using Tauron.AkkaHost;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Data;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Application.Settings;
using Tauron.Features;

namespace ServiceManager.Server
{
    public sealed class MainModule : AkkaModule
    {
        public override void Load(IActorApplicationBuilder builder)
        {
            builder.RegisterSettingsManager(c => c.WithProvider<LocalConfigurationProvider>());
            
            builder.RegisterEventDispatcher<IConfigEvent, ConfigEventDispatcher, ConfigApiEventDispatcherRef>(ConfigApiEventDispatcherActor.New(), "ConfigApiEventDispatcherActor");
            builder.RegisterEventDispatcher<AppInfo, AppEventDispatcher, AppEventDispatcherRef>(AppEventDispatcherActor.New(), "AppEventDispatcherRef");
            
            builder.RegisterFeature<ClusterNodeManagerRef>(ClusterHostManagerActor.New(), "ClusterHostManagerActor");
            builder.RegisterFeature<ProcessServiceHostRef>(ProcessServiceHostActor.New(), "ServiceDeamonHost");
            
            builder.RegisterStartUp<ActorStartUp>(s => s.Run());
        }

        public override void Load(IServiceCollection builder)
        {
            builder.AddSingleton<ILocalConfiguration, LocalConfiguration>();

            builder.AddSingleton<INodeRepository, NodeRepository>();
            builder.AddSingleton<IPropertyChangedNotifer, PropertyChangedNotifer>();

            builder.AddSingleton(c => DeploymentApi.CreateProxy(c.GetRequiredService<ActorSystem>()));
            builder.AddSingleton(c => RepositoryApi.CreateProxy(c.GetRequiredService<ActorSystem>()));
            builder.AddSingleton(c => ConfigurationApi.CreateProxy(c.GetRequiredService<ActorSystem>()));

            base.Load(builder);
        }
    }
}