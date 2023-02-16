using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Tauron.AkkaHost;
using Tauron.TAkka;

namespace Tauron.Application.Settings;

#pragma warning disable GU0011

[PublicAPI]
public static class SettingsManagerExtensions
{
    public static void RegisterSettingsManager(this IActorApplicationBuilder builder, Action<SettingsConfiguration>? config = null)
    {
        if(config != null)
        {
            var s = new SettingsConfiguration(builder);
            config(s);
        }

        builder.StartActors(
            (system, registry, resolver) =>
                registry.Register<SettingsManager>(system.ActorOf(resolver.Props<SettingsManager>(),"Settings-Manager")));
    }
}