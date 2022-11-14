using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Driver;

namespace Tauron.Application.Workshop.Mutation;

[PublicAPI]
public sealed class ExtendedMutatingEngine<TData> : MutatingEngine, IEventSourceable<TData>
    where TData : class
{
    private readonly IExtendedDataSource<TData> _dataSource;
    private readonly ResponderList _responder;

    internal ExtendedMutatingEngine(Action<IDataMutation> mutator, IExtendedDataSource<TData> dataSource, IDriverFactory driverFactory)
        : base(mutator, driverFactory)
    {
        _dataSource = dataSource;
        _responder = new ResponderList(dataSource.SetData, dataSource.OnCompled);
    }

    public IEventSource<TRespond> EventSource<TRespond>(
        Func<TData, TRespond> transformer,
        Func<TData, bool>? where = null)
        => new EventSource<TRespond, TData>(transformer, where, _responder);

    public IEventSource<TRespond> EventSource<TRespond>(Func<IObservable<TData>, IObservable<TRespond>> transform)
        => new EventSource<TRespond, TData>(transform(_responder));

    public void Mutate(string name, IQuery query, Func<IObservable<TData>, IObservable<TData?>> transform, IObserver<Unit>? onCompled = null)
        => Mutate(CreateMutate(name, query, transform, onCompled));

    public IDataMutation CreateMutate(string name, IQuery query, Func<IObservable<TData>, IObservable<TData?>> transform, IObserver<Unit>? onCompled = null)
    {
        async Task Runner()
        {
            var sender = new Subject<TData>();
            _responder.Push(query, transform(sender).NotNull(), onCompled);

            TData data = await _dataSource.GetData(query).ConfigureAwait(false);

            sender.OnNext(data);
            sender.OnCompleted();
        }

        return new AsyncDataMutation<TData>(Runner, name, query.ToHash());
    }

    private sealed class ResponderList : IObservable<TData>
    {
        private readonly Func<IQuery, Task> _completer;
        private readonly Subject<TData> _handler = new();
        private readonly Func<IQuery, TData, Task> _root;

        internal ResponderList(Func<IQuery, TData, Task> root, Func<IQuery, Task> completer)
        {
            _root = root;
            _completer = completer;
        }

        public IDisposable Subscribe(IObserver<TData> observer) => _handler.Subscribe(observer);

        internal void Push(IQuery query, IObservable<TData> dataFunc, IObserver<Unit>? onCompled)
        {
            var handler = dataFunc
               .SelectMany(
                    async data =>
                    {
                        try
                        {
                            await _root(query, data).ConfigureAwait(false);
                            _handler.OnNext(data);
                        }
                        finally
                        {
                            await _completer(query).ConfigureAwait(false);
                        }

                        return Unit.Default;
                    })
               .Timeout(TimeSpan.FromMinutes(10));

            if(onCompled is null)
                handler.Subscribe();
            else
                handler.SubscribeSafe(onCompled);
        }
    }
}