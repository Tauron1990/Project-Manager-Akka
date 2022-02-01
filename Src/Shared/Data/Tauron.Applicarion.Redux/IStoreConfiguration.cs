using JetBrains.Annotations;
using Tauron.Applicarion.Redux.Configuration;

namespace Tauron.Applicarion.Redux;

[PublicAPI]
public interface IStoreConfiguration
{
    IStoreConfiguration NewState<TState>(Func<ISourceConfiguration<TState>, IConfiguredState> configurator)
        where TState : class, new();

    IStoreConfiguration RegisterForFhinising(object toRegister);
    
    IRootStore Build();
}