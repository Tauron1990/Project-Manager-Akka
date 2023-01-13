using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Tauron.Application.Workshop.StateManagement.Akka.Internal;
using Tauron.Application.Workshop.StateManagement.Builder;

namespace Tauron.Application.Workshop.StateManagement.Akka;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
[PublicAPI]
public class AkkaStateModule : IModule
{
    #pragma warning disable GU0011
    public void Load(IServiceCollection collection)
    {
        collection.AddTransient<IStateInstanceFactory, FeatureBasedStateFactory>();
        collection.AddTransient<IStateInstanceFactory, ActorStateFactory>();
    }
}