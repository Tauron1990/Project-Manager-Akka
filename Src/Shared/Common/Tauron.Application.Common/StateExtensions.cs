using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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

    public static IObservable<TData> ToObservable<TData>(this IState<TData> state, Action<Exception> onError)
        => state.ToObservable(
            ex =>
            {
                onError(ex);
                return false;
            });

    public static IObservable<TData> ToObservable<TData>(this IState<TData> state, Func<Exception, bool> decidePropagateErrors)
        => Observable.Create<TData>(
                o =>
                {
                    if(state.HasValue)
                        o.OnNext(state.Value);

                    return new StateRegistration<TData>(o, state, decidePropagateErrors);
                })
           .DistinctUntilChanged()
           .Replay(1).RefCount();

    private sealed class StateRegistration<TData> : IDisposable
    {
        private readonly IObserver<TData> _observer;
        private readonly Func<Exception, bool> _skipErrors;
        private readonly IState<TData> _state;

        internal StateRegistration(IObserver<TData> observer, IState<TData> state, Func<Exception, bool> skipErrors)
        {
            _observer = observer;
            _state = state;
            _skipErrors = skipErrors;


            state.AddEventHandler(StateEventKind.All, Handler);
        }

        public void Dispose() => _state.RemoveEventHandler(StateEventKind.All, Handler);

        private void Handler(IState<TData> arg1, StateEventKind arg2)
        {
            if(_state.HasValue)
                _observer.OnNext(_state.Value);
            else if(_state.HasError && _state.Error is not null)
                if(_skipErrors(_state.Error))
                    _observer.OnError(_state.Error);
        }
    }
}