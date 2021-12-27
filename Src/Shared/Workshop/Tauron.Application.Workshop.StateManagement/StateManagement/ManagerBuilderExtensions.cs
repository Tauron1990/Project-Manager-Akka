using System.Reflection;
using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tauron.Application.Workshop.StateManagement.Builder;
using Tauron.Application.Workshop.StateManagement.DataFactorys;
using Tauron.Application.Workshop.StateManagement.Dispatcher.WorkDistributor;
using Tauron.Application.Workshop.StateManagement.Internal;

namespace Tauron.Application.Workshop.StateManagement;

[PublicAPI]
public static class ManagerBuilderExtensions
{
    public static IServiceCollection RegisterStateManager(this IServiceCollection collection, Action<ManagerBuilder, IServiceProvider> configAction)
        => RegisterStateManager(collection, new ServiceOptions(), configAction);

    public static IServiceCollection RegisterStateManager(this IServiceCollection collection, bool registerWorkspaceSuperviser, Action<ManagerBuilder, IServiceProvider> configAction)
        => RegisterStateManager(
            collection,
            new ServiceOptions { RegisterSuperviser = registerWorkspaceSuperviser },
            configAction);

    public static IServiceCollection RegisterStateManager(this IServiceCollection collection, ServiceOptions options, Action<ManagerBuilder, IServiceProvider> configAction)
    {
        if (options.RegisterSuperviser)
            collection.TryAddSingleton(c => new WorkspaceSuperviser(c.GetRequiredService<ActorSystem>(), "State_Manager_Superviser"));

        collection.TryAddSingleton<IActionInvoker>(
            context =>
            {
                var managerBuilder = new ManagerBuilder(context.GetRequiredService<WorkspaceSuperviser>())
                              {
                                  ServiceProvider = context
                              };

                configAction(managerBuilder, context);

                return managerBuilder.Build(options);
            });

        return collection;
    }

    public static ManagerBuilder AddFromAssembly<TType>(this ManagerBuilder builder, IDataSourceFactory factory)
        => AddFromAssembly(builder, typeof(TType).Assembly, factory);

    public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, Assembly assembly, IDataSourceFactory factory, CreationMetadata? metadata = null)
    {
        new ReflectionSearchEngine(Enumerable.Repeat(assembly, 1)).Add(builder, factory, metadata);

        return builder;
    }

    public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, IDataSourceFactory factory, CreationMetadata? metadata, IEnumerable<Assembly> assembly)
    {
        new ReflectionSearchEngine(assembly).Add(builder, factory, metadata);

        return builder;
    }


    public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, IDataSourceFactory factory, CreationMetadata? metadata, params Assembly[] assembly)
        => AddFromAssembly(builder, factory, metadata, assembly.AsEnumerable());

    public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, IDataSourceFactory factory, params Assembly[] assembly)
        => AddFromAssembly(builder, factory, null, assembly);

    public static ManagerBuilder AddFromAssembly<TType>(this ManagerBuilder builder, IServiceProvider context)
        => AddFromAssembly(builder, typeof(TType).Assembly, context);

    public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, IDataSourceFactory factory, CreationMetadata? metadata, params Type[] assembly)
        => AddFromAssembly(builder, factory, metadata, assembly.Select(t => t.Assembly));

    public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, IDataSourceFactory factory, params Type[] assembly)
        => AddFromAssembly(builder, factory, null, assembly.Select(t => t.Assembly));


    public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, Assembly assembly, IServiceProvider context)
        => AddFromAssembly(
            builder,
            assembly,
            MergeFactory.Merge(
                context.GetRequiredService<IEnumerable<IDataSourceFactory>>().Cast<AdvancedDataSourceFactory>()
                   .ToArray()));

    public static ManagerBuilder AddTypes(this ManagerBuilder builder, IDataSourceFactory factory, CreationMetadata? metadata, IEnumerable<Type> types)
    {
        new ReflectionSearchEngine(types).Add(builder, factory, metadata);

        return builder;
    }

    public static ManagerBuilder AddTypes(this ManagerBuilder builder, IDataSourceFactory factory, CreationMetadata? metadata, params Type[] types)
        => AddTypes(builder, factory, metadata, (IEnumerable<Type>)types);

    public static ManagerBuilder AddTypes(this ManagerBuilder builder, IDataSourceFactory factory, params Type[] types)
        => AddTypes(builder, factory, null, types);

    public static IConcurrentDispatcherConfugiration WithConcurentDispatcher<TReturn>(this IDispatcherConfigurable<TReturn> builder)
        where TReturn : IDispatcherConfigurable<TReturn>
    {
        var config = new ConcurrentDispatcherConfugiration();
        builder.WithDispatcher(config.Create);

        return config;
    }

    public static IConsistentHashDispatcherPoolConfiguration WithConsistentHashDispatcher<TReturn>(this IDispatcherConfigurable<TReturn> builder)
        where TReturn : IDispatcherConfigurable<TReturn>
    {
        var config = new ConsistentHashDispatcherConfiguration();
        builder.WithDispatcher(config.Create);

        return config;
    }

    public static TType WithWorkDistributorDispatcher<TType>(this TType source, TimeSpan? timeout = null)
        where TType : IDispatcherConfigurable<TType>
    {
        timeout ??= TimeSpan.FromMinutes(1);

        return source.WithDispatcher(() => new WorkDistributorConfigurator(timeout.Value));
    }

    public static IConsistentHashDispatcherPoolConfiguration WithConsistentHashDispatcher<TReturn>(this IDispatcherConfigurable<TReturn> builder, string name)
        where TReturn : IDispatcherConfigurable<TReturn>
    {
        var config = new ConsistentHashDispatcherConfiguration();
        builder.WithDispatcher(name, config.Create);

        return config;
    }

    public static TType WithWorkDistributorDispatcher<TType>(this TType source, string name, TimeSpan? timeout = null)
        where TType : IDispatcherConfigurable<TType>
    {
        timeout ??= TimeSpan.FromMinutes(1);

        return source.WithDispatcher(name, () => new WorkDistributorConfigurator(timeout.Value));
    }


    public static IConcurrentDispatcherConfugiration WithConcurentDispatcher<TReturn>(this IDispatcherConfigurable<TReturn> builder, string name)
        where TReturn : IDispatcherConfigurable<TReturn>
    {
        var config = new ConcurrentDispatcherConfugiration();
        builder.WithDispatcher(name, config.Create);

        return config;
    }
}