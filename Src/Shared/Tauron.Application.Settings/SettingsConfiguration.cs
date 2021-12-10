﻿using System;
using Autofac;
using JetBrains.Annotations;

namespace Tauron.Application.Settings;

[PublicAPI]
public sealed class SettingsConfiguration
{
    private readonly ContainerBuilder _builder;

    public SettingsConfiguration(ContainerBuilder builder) => _builder = builder;

    public SettingsConfiguration WithProvider<TType>()
        where TType : ISettingProviderConfiguration
    {
        _builder.RegisterType<TType>().As<ISettingProviderConfiguration>();

        return this;
    }

    public SettingsConfiguration WithProvider<TType>(Func<TType> setting)
        where TType : ISettingProviderConfiguration
    {
        _builder.Register(_ => setting()).As<ISettingProviderConfiguration>();

        return this;
    }
}