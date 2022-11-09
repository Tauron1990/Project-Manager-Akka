using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tauron.Application.Workshop.Driver;
using Tauron.Application.Workshop.StateManagement.DataFactorys;
using Tauron.Application.Workshop.StateManagement.Internal;

namespace Tauron.Application.Workshop.StateManagement;

[PublicAPI]
public static class ManagerBuilderExtensions
{
    public static IServiceCollection RegisterStateManager(this IServiceCollection collection, Action<ManagerBuilder, IServiceProvider> configAction)
        => RegisterStateManager(collection, new ServiceOptions(), configAction);

    public static IServiceCollection RegisterStateManager(this IServiceCollection collection, ServiceOptions options, Action<ManagerBuilder, IServiceProvider> configAction)
    {
        collection.TryAddSingleton<IActionInvoker>(
            context =>
            {
                var managerBuilder = new ManagerBuilder(context.GetRequiredService<IDriverFactory>())
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
}