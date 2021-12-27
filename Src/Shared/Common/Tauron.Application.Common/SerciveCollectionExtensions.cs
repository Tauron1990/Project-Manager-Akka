using System;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;

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
    public static IServiceCollection ScanModules(this IServiceCollection collection, Predicate<Assembly>? predicate = null)
    {
        foreach (var module in from lib in DependencyContext.Default.RuntimeLibraries
                              from name in lib.GetDefaultAssemblyNames(DependencyContext.Default)
                              let assembly = TryLoad(name)
                              where assembly is not null && (predicate?.Invoke(assembly) ?? true)
                              from type in assembly.ExportedTypes
                              where type.IsAssignableTo(typeof(IModule))
                              let inst = TryActivate(type)
                              where inst is not null
                              select inst)
        {
            module.Load(collection);
        }

        return collection;
    }

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
        catch (Exception e) when(e is ArgumentException or FileNotFoundException or FileLoadException or BadImageFormatException)
        {
            return null;
        }
    }
    
    public static IServiceCollection RegisterModule<TModule>(this IServiceCollection collection)
        where TModule : class, IModule
        => RegisterModule(collection, Activator.CreateInstance<TModule>());

    public static IServiceCollection RegisterModule(this IServiceCollection collection, IModule module)
    {
        module.Load(collection);

        return collection;
    }
    
    public static IServiceCollection WhenNotRegistered<TService>(this IServiceCollection collection, Action<IServiceCollection> register)
    {
        if(collection.Any(s => s.ServiceType == typeof(TService))) return collection;

        register(collection);

        return collection;
    }
}