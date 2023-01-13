using JetBrains.Annotations;

namespace Tauron.Application.Akka.Redux.Configuration;

[PublicAPI]
public interface IStoreConfiguration
{
    IStoreConfiguration NewState<TState>(Func<ISourceConfiguration<TState>, IConfiguredState> configurator)
        where TState : class, new();

    IStoreConfiguration RegisterForFhinising(object toRegister);

    IRootStore Build();
}