using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Stl.Fusion;
using Tauron.Applicarion.Redux.Extensions.Cache;
using Tauron.Applicarion.Redux.Internal.Configuration;

namespace Tauron.Applicarion.Redux;

[PublicAPI]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStoreConfiguration(this IServiceCollection coll)
    {
        coll.AddTransient<TimeoutManager>()
           .AddSingleton<StateDb>()
           .AddTransient<IStoreConfiguration, StoreConfiguration>();
        coll.AddFusion();

        return coll;
    }

    public static IServiceCollection AddRootStore(this IServiceCollection coll, Action<IStoreConfiguration> configurate)
        => coll
           .AddStoreConfiguration()
           .AddSingleton(
                s =>
                {
                    var config = s.GetRequiredService<IStoreConfiguration>();
                    configurate(config);

                    return config.Build();
                });
}