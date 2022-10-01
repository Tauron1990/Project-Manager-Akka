using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable GU0011

namespace Tauron.Application.Settings;

[PublicAPI]
public sealed class SettingsConfiguration
{
    private readonly IServiceCollection _builder;

    public SettingsConfiguration(IServiceCollection builder) => _builder = builder;

    public SettingsConfiguration WithProvider<TType>()
        where TType : class, ISettingProviderConfiguration
    {
        _builder.AddScoped<ISettingProviderConfiguration, TType>();

        return this;
    }

    public SettingsConfiguration WithProvider<TType>(Func<TType> setting)
        where TType : ISettingProviderConfiguration
    {
        _builder.AddScoped<ISettingProviderConfiguration>(_ => setting());

        return this;
    }
    
    public SettingsConfiguration WithProvider<TType>(Func<IServiceProvider, TType> setting)
        where TType : ISettingProviderConfiguration
    {
        _builder.AddScoped<ISettingProviderConfiguration>(p => setting(p));

        return this;
    }
}