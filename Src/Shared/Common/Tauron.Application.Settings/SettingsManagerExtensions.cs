using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Tauron.TAkka;

namespace Tauron.Application.Settings;

[PublicAPI]
public static class SettingsManagerExtensions
{
    public static void RegisterSettingsManager(this IServiceCollection builder, Action<SettingsConfiguration>? config = null)
    {
        if (config != null)
        {
            var s = new SettingsConfiguration(builder);
            config(s);
        }

        builder.AddSingleton<IDefaultActorRef<SettingsManager>>(
            c =>
            {
                var man = new DefaultActorRef<SettingsManager>(c.GetRequiredService<ActorRefFactory<SettingsManager>>());
                man.Init("Settings-Manager");

                return man;
            });
    }
}