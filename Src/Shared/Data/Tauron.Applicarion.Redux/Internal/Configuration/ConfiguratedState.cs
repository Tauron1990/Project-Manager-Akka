using Tauron.Applicarion.Redux.Configuration;
using Tauron.Applicarion.Redux.Extensions.Internal;

namespace Tauron.Applicarion.Redux.Internal.Configuration;

public sealed class ConfiguratedState : IConfiguredState
{
    private readonly Type _stateType;
    private readonly Guid _id;
    private readonly Action<IRootStore>? _onCreate;

    public ConfiguratedState(Type stateType, Guid id, Action<IRootStore>? onCreate)
    {
        _stateType = stateType;
        _id = id;
        _onCreate = onCreate;
    }
    
    public void RunConfig(IReduxStore<MultiState> store, Action<Type, Guid> registerState)
        => registerState(_stateType, _id);

    public void PostBuild(IRootStore store)
        => _onCreate?.Invoke(store);
}