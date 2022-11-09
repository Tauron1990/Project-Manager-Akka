using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Tauron.Localization.Provider;

namespace Tauron.Localization;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
public class LocalicationModule : IModule
{
    public void Load(IServiceCollection collection)
        => collection.AddScoped<ILocStoreProducer, LocJsonProvider>();
}