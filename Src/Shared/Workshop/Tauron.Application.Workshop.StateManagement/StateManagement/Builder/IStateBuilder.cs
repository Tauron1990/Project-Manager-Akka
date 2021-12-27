using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement.Builder;

[PublicAPI]
public interface IStateBuilder<TData> : IDispatcherConfigurable<IStateBuilder<TData>>
    where TData : class
{
    IStateBuilder<TData> WithStateType<TState>();

    IStateBuilder<TData> WithStateType(Type type);

    IStateBuilder<TData> WithReducer(Func<IReducer<TData>> reducer);

    IStateBuilder<TData> WithKey(string key);
}