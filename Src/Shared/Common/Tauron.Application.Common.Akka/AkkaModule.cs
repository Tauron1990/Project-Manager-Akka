using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tauron.TAkka;

namespace Tauron;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
[UsedImplicitly]
public class AkkaModule : IModule
{
    public void Load(IServiceCollection collection)
    {
        collection.TryAddScoped(typeof(ActorRefFactory<>));
        collection.TryAddScoped(typeof(IDefaultActorRef<>), typeof(DefaultActorRef<>));
        collection.TryAddScoped(typeof(ISyncActorRef<>), typeof(SyncActorRef<>));
    }
}