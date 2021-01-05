using System;
using Tauron.Application.Workshop.Mutating;

namespace Tauron.Application.Workshop.StateManagement
{
    public interface IReducer<TData>
    {
        Func<IObservable<MutatingContext<TData>>, IObservable<ReducerResult<TData>>> Reduce(IStateAction action);

        bool ShouldReduceStateForAction(IStateAction action);
    }
}