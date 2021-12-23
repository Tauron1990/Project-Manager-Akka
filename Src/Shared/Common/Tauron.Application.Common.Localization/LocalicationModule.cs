using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Tauron.Localization.Provider;
using Tauron.Modules;

namespace Tauron.Application.Common.Localization;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
public class LocalicationModule : IModule
{
    public void Load(IServiceCollection collection)
        => collection.AddScoped< ILocStoreProducer, LocJsonProvider>();
}