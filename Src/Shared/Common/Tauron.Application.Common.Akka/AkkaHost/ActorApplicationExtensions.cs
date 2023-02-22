using System.Reflection;
using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Hosting;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tauron.AkkaHost;

[PublicAPI]
public static class ActorApplicationExtensions
{
    [PublicAPI]
    public static IHostBuilder ConfigureAkkaApplication(this IHostBuilder hostBuilder, Action<IActorApplicationBuilder>? config = null)
    {
        var builder = new ActorApplicationBluilder(hostBuilder);
        Module.Internal.HandlerRegistry.ModuleHandler = new ActorApplicationHandler(builder);
        
        config?.Invoke(builder);

        return hostBuilder.UseServiceProviderFactory(
            builderContext =>
            {
                builder.Init(builderContext);

                return new ActorServiceProviderFactory(builder);
            });
    }

    public static IActorApplicationBuilder ConfigureAkka(this IActorApplicationBuilder builder, Action<AkkaConfigurationBuilder> config)
        => builder.ConfigureAkka((_, _, c) => config(c));

    public static IActorApplicationBuilder StartActors(this IActorApplicationBuilder builder, Action<ActorSystem, IActorRegistry> starter)
        => builder.ConfigureAkka(b => b.StartActors(starter));
    
    public static IActorApplicationBuilder StartActors(this IActorApplicationBuilder builder, Action<ActorSystem, IActorRegistry, IDependencyResolver> starter)
        => builder.ConfigureAkka(b => b.StartActors(starter));

    public static IActorApplicationBuilder StartActors(this IActorApplicationBuilder builder, ActorStarter starter)
        => builder.ConfigureAkka(b => b.StartActors(starter));

    public static IActorApplicationBuilder StartActors(this IActorApplicationBuilder builder, ActorStarterWithResolver starter)
        => builder.ConfigureAkka(b => b.StartActors(starter));
    
    public static IActorApplicationBuilder ConfigureServices(this IActorApplicationBuilder builder, Action<IServiceCollection> config)
        => builder.ConfigureServices((_, sc) => config(sc));

    public static IActorApplicationBuilder ScanModules(this IActorApplicationBuilder builder, IEnumerable<Assembly> assemblies)
        => builder.ConfigureHost(s => s.ScanModules(assemblies));

    public static IActorApplicationBuilder ScanModules(this IActorApplicationBuilder builder, Predicate<Assembly>? predicate = null)
        => builder.ConfigureHost(s => s.ScanModules(predicate));


    public static IActorApplicationBuilder RegisterModule<TModule>(this IActorApplicationBuilder builder)
        where TModule : class, IModule, new()
        => builder.ConfigureHost(s => s.RegisterModule<TModule>());

    public static IActorApplicationBuilder RegisterModule(this IActorApplicationBuilder builder, IModule module)
        => builder.ConfigureHost(s => s.RegisterModule(module));

    public static IActorApplicationBuilder RegisterModules(this IActorApplicationBuilder builder, params IModule[] modules)
        => builder.ConfigureHost(s => s.RegisterModules(modules));
}