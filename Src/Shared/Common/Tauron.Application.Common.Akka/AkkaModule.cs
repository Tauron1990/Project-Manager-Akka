using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Tauron.TAkka;

namespace Tauron;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
public class AkkaModule : IModule
{
    public void Load(IServiceCollection collection)
    {
        collection.AddScoped(typeof(ActorRefFactory<>));
        collection.AddScoped(typeof(IDefaultActorRef<>), typeof(DefaultActorRef<>));
        collection.AddScoped(typeof(ISyncActorRef<>), typeof(SyncActorRef<>));
    }
}