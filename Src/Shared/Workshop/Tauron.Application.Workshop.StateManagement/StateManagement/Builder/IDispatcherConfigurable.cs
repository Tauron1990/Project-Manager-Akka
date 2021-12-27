using Tauron.Application.Workshop.StateManagement.Dispatcher;

namespace Tauron.Application.Workshop.StateManagement.Builder;

public interface IDispatcherConfigurable<out TThis>
    where TThis : IDispatcherConfigurable<TThis>
{
    public TThis WithDispatcher(Func<IStateDispatcherConfigurator>? factory);

    public TThis WithDispatcher(string name, Func<IStateDispatcherConfigurator>? factory);
}