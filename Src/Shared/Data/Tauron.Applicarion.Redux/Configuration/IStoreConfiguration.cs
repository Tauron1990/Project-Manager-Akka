﻿using System.Reactive.Concurrency;
using JetBrains.Annotations;

namespace Tauron.Applicarion.Redux.Configuration;

[PublicAPI]
public interface IStoreConfiguration
{
    IStoreConfiguration NewState<TState>(Func<ISourceConfiguration<TState>, IConfiguredState> configurator)
        where TState : class, new();

    IStoreConfiguration RegisterForFhinising(object toRegister);

    IRootStore Build(IScheduler? scheduler = null);
}