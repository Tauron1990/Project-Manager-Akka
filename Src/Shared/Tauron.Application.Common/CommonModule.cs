using System.Reactive.PlatformServices;
using Autofac;
using Autofac.Features.ResolveAnything;
using Stl.Time;
using Tauron.Application;
using Tauron.Application.VirtualFiles;
using Tauron.Localization.Provider;
using Tauron.TAkka;

namespace Tauron;

public sealed class CommonModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<VirtualFileFactory>();
        builder.RegisterInstance(CpuClock.Instance).As<IMomentClock, ISystemClock>().SingleInstance();

        builder.RegisterSource<AnyConcreteTypeNotAlreadyRegisteredSource>();
        builder.RegisterType<LocJsonProvider>().As<ILocStoreProducer>();

        builder.RegisterGeneric(typeof(ActorRefFactory<>)).AsSelf();
        builder.RegisterGeneric(typeof(DefaultActorRef<>)).As(typeof(IDefaultActorRef<>));
        builder.RegisterGeneric(typeof(SyncActorRef<>)).As(typeof(ISyncActorRef<>));

        builder.RegisterType<TauronEnviromentImpl>().As<ITauronEnviroment>().SingleInstance();
        builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
    }
}