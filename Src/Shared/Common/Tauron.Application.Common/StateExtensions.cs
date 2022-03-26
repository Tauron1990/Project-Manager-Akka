using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Stl;
using Stl.Fusion;

namespace Tauron;

public static class StateExtensions
{
    public static IDisposableState<TData> ToState<TData>(this IObservable<TData> input, IStateFactory factory)
    {
        var serial = new SerialDisposable();
        var state = factory.NewMutable(new MutableState<TData>.Options());
        serial.Disposable = input.AutoSubscribe(n => state.Set(n), () => serial.Dispose(), e => state.Set(Result.Error<TData>(e)));

        return new DisposableState<TData>(state, serial);
    }

    public static IObservable<TData> ToObservable<TData>(this IState<TData> state, bool skipErrors = false)
        => Observable.Create<TData>(o =>
                                    {
                                        if(state.HasValue)
                                            o.OnNext(state.Value);
                                        return new StateRegistration<TData>(o, state, skipErrors);
                                    })
           .DistinctUntilChanged();
    
    private sealed class StateRegistration<TData> : IDisposable
    {
        private readonly IObserver<TData> _observer;
        private readonly IState<TData> _state;
        private readonly bool _skipErrors;

        internal StateRegistration(IObserver<TData> observer, IState<TData> state, bool skipErrors)
        {
            _observer = observer;
            _state = state;
            _skipErrors = skipErrors;

            
            
            state.AddEventHandler(StateEventKind.All, Handler);
        }

        private void Handler(IState<TData> arg1, StateEventKind arg2)
        {
            if(_state.HasValue)
                _observer.OnNext(_state.Value);
            else if(!_skipErrors && _state.HasError && _state.Error is not null)
                _observer.OnError(_state.Error);
        }

        public void Dispose() => _state.RemoveEventHandler(StateEventKind.All, Handler);
    }
    
}