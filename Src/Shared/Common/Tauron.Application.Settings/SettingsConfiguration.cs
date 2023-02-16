using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Tauron.AkkaHost;

#pragma warning disable GU0011

namespace Tauron.Application.Settings;

[PublicAPI]
public sealed class SettingsConfiguration
{
    private readonly IActorApplicationBuilder _builder;

    public SettingsConfiguration(IActorApplicationBuilder builder) => _builder = builder;

    public SettingsConfiguration WithProvider<TType>()
        where TType : class, ISettingProviderConfiguration
    {
        _builder.ConfigureServices(s => s.AddScoped<ISettingProviderConfiguration, TType>());

        return this;
    }

    public SettingsConfiguration WithProvider<TType>(Func<TType> setting)
        where TType : class, ISettingProviderConfiguration
    {
        _builder.ConfigureServices(s => s.AddScoped<ISettingProviderConfiguration>(_ => setting()));

        return this;
    }

    public SettingsConfiguration WithProvider<TType>(Func<IServiceProvider, TType> setting)
        where TType : class, ISettingProviderConfiguration
    {
        _builder.ConfigureServices(s => s.AddScoped<ISettingProviderConfiguration>(setting));

        return this;
    }
}