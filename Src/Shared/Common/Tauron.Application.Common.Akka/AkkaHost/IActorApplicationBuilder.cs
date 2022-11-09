using Akka.Hosting;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tauron.AkkaHost;

[PublicAPI]
public interface IActorApplicationBuilder
{
    // IActorApplicationBuilder ConfigureAkka(Func<HostBuilderContext, Config> config);
    //
    // IActorApplicationBuilder ConfigureAkka(Func<HostBuilderContext, Setup> config);

    IActorApplicationBuilder ConfigureAkka(Action<HostBuilderContext, AkkaConfigurationBuilder> system);

    IActorApplicationBuilder ConfigureAkka(Action<HostBuilderContext, IServiceProvider, AkkaConfigurationBuilder> system);

    IActorApplicationBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate);

    IActorApplicationBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate);

    IActorApplicationBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate);

    IActorApplicationBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate);
}