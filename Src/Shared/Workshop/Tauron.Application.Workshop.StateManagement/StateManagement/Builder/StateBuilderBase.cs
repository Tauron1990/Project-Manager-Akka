using Tauron.Application.Workshop.StateManagement.Internal;

namespace Tauron.Application.Workshop.StateManagement.Builder;

public abstract class StateBuilderBase
{
    public abstract (StateContainer State, string Key) Materialize(StateBuilderParameter parameter);
}