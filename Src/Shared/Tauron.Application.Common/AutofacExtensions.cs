using System;
using Autofac;
using JetBrains.Annotations;

namespace Tauron;

[PublicAPI]
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
}