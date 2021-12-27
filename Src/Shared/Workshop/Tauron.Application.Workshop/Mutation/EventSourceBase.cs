using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.Mutation;

[PublicAPI]
public abstract class EventSourceBase<TRespond> : IEventSource<TRespond>
{
    private static IEventSource<TRespond>? _empty;

    private readonly Subject<TRespond> _subject = new();

    public static IEventSource<TRespond> Empty => _empty ??= new EmptyEventSource();


    public IDisposable Subscribe(IObserver<TRespond> observer) => _subject.Subscribe(observer);

    protected IObserver<TRespond> Sender() => _subject.AsObserver();

    private sealed class EmptyEventSource : IEventSource<TRespond>
    {
        public IDisposable Subscribe(IObserver<TRespond> observer) => Disposable.Empty;
    }
}