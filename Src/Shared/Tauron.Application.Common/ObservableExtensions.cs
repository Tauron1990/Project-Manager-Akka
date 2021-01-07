using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Akka.Actor;
using JetBrains.Annotations;
using Serilog;
using Serilog.Context;
using Tauron.Akka;

namespace Tauron
{
    [PublicAPI]
    public static class ObservableExtensions
    {
        //private sealed class SingleTimeObserver<TEvent> : IObserver<TEvent>, IDisposable
        //{
        //    private readonly object _gate = new();
        //    private IDisposable? _dis;
        //    private int _runed;

        //    private Action<TEvent>? Handler { get; }

        //    public Action<Exception>? Error { get; init; }

        //    public Action? OnCompled { get; init; }
            
        //    public SingleTimeObserver(Action<TEvent>? handler) => Handler = handler;

        //    public SingleTimeObserver() { }

        //    public IDisposable Register(IObservable<TEvent> evt)
        //    {
        //        lock (_gate)
        //        {
        //            _dis = evt.Subscribe(this);
        //            if (_runed == 1)
        //                _dis?.Dispose();
        //        }

        //        return _dis ?? Disposable.Empty;
        //    }
            
        //    public void OnCompleted()
        //    {
        //        _dis?.Dispose();
        //        OnCompled?.Invoke();
        //    }

        //    public void OnError(Exception error)
        //    {
        //        _dis?.Dispose();
             
        //        if(Error == null)
        //            Log.ForContext(typeof(SingleTimeObserver<>)).Error(error, "Unobserved Exception occurred");
        //            else
        //            Error?.Invoke(error);
        //    }

        //    public void OnNext(TEvent value)
        //    {
        //        Handler?.Invoke(value);

        //        lock (_gate)
        //        {
        //            _dis?.Dispose();
        //            _dis = null;
        //            _runed = 1;
        //        }
        //    }

        //    public void Dispose() 
        //        => _dis?.Dispose();
        //}

        //public static IDisposable SingleTimeSubscribe<TEvent>(this IObservable<TEvent> observable)
        //{
        //    var observer = new SingleTimeObserver<TEvent>();

        //    return observer.Register(observable);
        //}

        //public static IDisposable SingleTimeSubscribe<TEvent>(this IObservable<TEvent> observable, Action<TEvent>? handler, Action<Exception>? error, Action? onCompled)
        //{
        //    var observer = new SingleTimeObserver<TEvent>(handler)
        //                   {
        //                       Error = error,
        //                       OnCompled = onCompled
        //                   };

        //    return observer.Register(observable);
        //}

        //public static IDisposable SingleTimeSubscribe<TEvent>(this IObservable<TEvent> observable, Action<TEvent>? handler, Action<Exception>? error)
        //    => SingleTimeSubscribe(observable, handler, error, null);

        //public static IDisposable SingleTimeSubscribe<TEvent>(this IObservable<TEvent> observable, Action<TEvent>? handler)
        //    => SingleTimeSubscribe(observable, handler, null);

        //public static IDisposable Subscribe<TEvent>(this IObservable<TEvent> observable, IActorRef actor) 
        //    => observable.Subscribe(evt => actor.Tell(evt), e => actor.Tell(new Status.Failure(e)), () => actor.Tell(new Status.Success(Unit.Default)));
        




        public static IObservable<IActorRef> NotNobody(this IObservable<IActorRef> observable) 
            => observable.Where(a => !a.IsNobody());

        public static IObservable<Unit> ToUnit<TSource>(this IObservable<TSource> input)
            => input.Select(_ => Unit.Default);

        public static IObservable<TType> Isonlate<TType>(this IObservable<TType> obs) => obs.Publish().RefCount();


        public static IDisposable ToSelf<TMessage>(this IObservable<TMessage> obs)
            => ToActor(obs, ExpandedReceiveActor.ExposedContext.Self);

        public static IDisposable ToParent<TMessage>(this IObservable<TMessage> source)
            => ToParent(source, ExpandedReceiveActor.ExposedContext);

        public static IDisposable ToParent<TMessage>(this IObservable<TMessage> source, IUntypedActorContext context) 
            => source.Subscribe(m => context.Parent.Tell(m));

        public static IDisposable ToSender<TMessage>(this IObservable<TMessage> source)
            => ToSender(source, ExpandedReceiveActor.ExposedContext);

        public static IDisposable ToSender<TMessage>(this IObservable<TMessage> source, IUntypedActorContext context)
            => source.Subscribe(m => context.Sender.Tell(m));
        
        public static IDisposable ToActor<TMessage>(this IObservable<TMessage> source, IActorRef target)
            => source.Subscribe(m => target.Tell(m));

        public static IDisposable ToActor<TMessage>(this IObservable<TMessage> source, Func<IActorRef> target)
            => source.Subscribe(m => target().Tell(m));

        public static IDisposable ToActor<TMessage>(this IObservable<TMessage> source, Func<IUntypedActorContext, IActorRef> target)
            => source.Subscribe(m => target(ExpandedReceiveActor.ExposedContext).Tell(m));

        public static IDisposable ToActor<TMessage>(this IObservable<TMessage> source, Func<TMessage, IActorRef> target)
            => source.Subscribe(m => target(m).Tell(m));


        public static IDisposable ForwardToParent<TMessage>(this IObservable<TMessage> source)
            => ForwardToParent(source, ExpandedReceiveActor.ExposedContext);

        public static IDisposable ForwardToParent<TMessage>(this IObservable<TMessage> source, IUntypedActorContext context)
            => source.Subscribe(m => context.Parent.Forward(m));

        public static IDisposable ForwardToSender<TMessage>(this IObservable<TMessage> source)
            => ForwardToSender(source, ExpandedReceiveActor.ExposedContext);

        public static IDisposable ForwardToSender<TMessage>(this IObservable<TMessage> source, IUntypedActorContext context)
            => source.Subscribe(m => context.Sender.Forward(m));

        public static IDisposable ForwardToActor<TMessage>(this IObservable<TMessage> source, IActorRef target)
            => source.Subscribe(m => target.Forward(m));

        public static IDisposable ForwardToActor<TMessage>(this IObservable<TMessage> source, Func<IActorRef> target)
            => source.Subscribe(m => target().Forward(m));

        public static IDisposable ForwardToActor<TMessage>(this IObservable<TMessage> source, Func<IUntypedActorContext, IActorRef> target)
            => source.Subscribe(m => target(ExpandedReceiveActor.ExposedContext).Forward(m));

        public static IDisposable ForwardToActor<TMessage>(this IObservable<TMessage> source, Func<TMessage, IActorRef> target)
            => source.Subscribe(m => target(m).Forward(m));

    }
}