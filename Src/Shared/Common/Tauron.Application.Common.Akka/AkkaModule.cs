using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Tauron.TAkka;

namespace Tauron;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
[UsedImplicitly]
public class AkkaModule : IModule
{
    public void Load(IServiceCollection collection)
    {
        collection.AddScoped(typeof(ActorRefFactory<>));
        collection.AddScoped(typeof(IDefaultActorRef<>), typeof(DefaultActorRef<>));
        collection.AddScoped(typeof(ISyncActorRef<>), typeof(SyncActorRef<>));
    }
}