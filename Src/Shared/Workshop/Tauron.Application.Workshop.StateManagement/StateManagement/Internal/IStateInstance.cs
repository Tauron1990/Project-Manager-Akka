using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.Internal;

public interface IStateInstance
{
    object ActualState { get; }

    void InitState<TData>(ExtendedMutatingEngine<MutatingContext<TData>> engine);

    void ApplyQuery<TData>(IExtendedDataSource<MutatingContext<TData>> engine)
        where TData : class, IStateEntity;

    void PostInit(IActionInvoker actionInvoker);
}