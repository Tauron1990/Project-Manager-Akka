using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Tauron.Module.Internal;

namespace Tauron;

/*[PublicAPI]
public static class AutofacExtensions
{
    public static void WhenNotRegistered<TService>(this ContainerBuilder builder, Action<ContainerBuilder> register)
    {
        var startableType = typeof(TService);
        var startableTypeKey = $"PreventDuplicateRegistration({startableType.FullName})";

        if (builder.Properties.ContainsKey(startableTypeKey))
            return;
        
        builder.Properties.Add(startableTypeKey, null);
        register(builder);
    }
}*/

[PublicAPI]
public static class SerciveCollectionExtensions
{
    public static IHostBuilder ScanModules(this IHostBuilder collection, IEnumerable<Assembly> assemblies)
    {
        foreach (IModule? module in from lib in assemblies
                                    from type in lib.ExportedTypes
                                    where type.IsAssignableTo(typeof(IModule))
                                    let inst = TryActivate(type)
                                    where inst is not null
                                    select inst)
            HandlerRegistry.ModuleHandler.Handle(collection, module);

        return collection;
    }

    public static IHostBuilder ScanModules(this IHostBuilder collection, Predicate<Assembly>? predicate = null)
        => ScanModules(
            collection,
            from lib in DependencyContext.Default.RuntimeLibraries
            from name in lib.GetDefaultAssemblyNames(DependencyContext.Default)
            let assembly = TryLoad(name)
            where assembly is not null && (predicate?.Invoke(assembly) ?? true)
            select assembly);

    private static IModule? TryActivate(Type type)
    {
        try
        {
            return Activator.CreateInstance(type) as IModule;
        }
        catch (Exception e) when (
            e is ArgumentException or NotSupportedException or TargetInvocationException
                or MethodAccessException or MemberAccessException or MissingMethodException)
        {
            return null;
        }
    }

    private static Assembly? TryLoad(AssemblyName name)
    {
        try
        {
            return Assembly.Load(name);
        }
        catch (Exception e) when (e is ArgumentException or FileNotFoundException or FileLoadException or BadImageFormatException)
        {
            return null;
        }
    }

    public static IHostBuilder RegisterModule<TModule>(this IHostBuilder collection)
        where TModule : class, IModule, new()
        => RegisterModule(collection, new TModule());

    public static IHostBuilder RegisterModule(this IHostBuilder collection, IModule module)
    {
        HandlerRegistry.ModuleHandler.Handle(collection, module);

        return collection;
    }

    public static IHostBuilder RegisterModules(this IHostBuilder collection, params IModule[] modules)
    {
        foreach (var module in modules) HandlerRegistry.ModuleHandler.Handle(collection, module);

        return collection;
    }

    public static IServiceCollection WhenNotRegistered<TService>(this IServiceCollection collection, Action<IServiceCollection> register)
    {
        if(collection.Any(s => s.ServiceType == typeof(TService))) return collection;

        register(collection);

        return collection;
    }
}