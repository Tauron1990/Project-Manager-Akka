using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Stl.Fusion;
using Tauron.Application.Akka.Redux.Configuration;
using Tauron.Application.Akka.Redux.Extensions.Cache;
using Tauron.Application.Akka.Redux.Internal.Configuration;

namespace Tauron.Application.Akka.Redux;

[PublicAPI]
public static class ServiceCollectionExtensions
{
    #pragma warning disable GU0011
    
    public static IServiceCollection AddStoreConfiguration(this IServiceCollection coll)
    {
        coll.AddTransient<TimeoutManager>()
           .AddScoped<StateDb>()
           .AddTransient<IStoreConfiguration, StoreConfiguration>();
        coll.AddFusion();

        return coll;
    }

    public static IServiceCollection AddRootStore(this IServiceCollection coll, Action<IServiceProvider, IStoreConfiguration> configurate)
        => coll
           .AddStoreConfiguration()
           .AddSingleton(
                s =>
                {
                    var config = s.GetRequiredService<IStoreConfiguration>();
                    configurate(s, config);

                    return config.Build();
                });
}