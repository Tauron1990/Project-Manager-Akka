using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Analyzing;
using Tauron.Application.Workshop.Driver;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutating.Changes;
using Tauron.Operations;

namespace Tauron.Application.Workshop.Mutation;

[PublicAPI]
public sealed class MutatingEngine<TData> : MutatingEngine, IEventSourceable<TData>
    where TData : class
{
    private class DummyFactory : IDriverFactory
    {
        public Action<IDataMutation> CreateMutator()
            => _ => {};

        public Action<RegisterRule<TWorkspace, TData1>> CreateAnalyser<TWorkspace, TData1>(TWorkspace workspace, IObserver<RuleIssuesChanged<TWorkspace, TData1>> observer) where TWorkspace : WorkspaceBase<TData1> where TData1 : class
            => _ => {};

        public void CreateProcessor(Type processor, string name)
        {
            
        }

        public Action<IOperationResult> GetResultSender()
            => _ => {};
    }

    private readonly IDataSource<TData> _dataSource;
    private readonly Subject<TData> _responder;

    internal MutatingEngine(Action<IDataMutation> mutator, IDataSource<TData> dataSource, IDriverFactory driverFactory)
        : base(mutator, driverFactory)
    {
        _dataSource = dataSource;
        _responder = new Subject<TData>();
        _responder.Subscribe(dataSource.SetData);
    }

    public MutatingEngine(IDataSource<TData> dataSource) 
        : base(_ => {}, new DummyFactory())
    {
        _dataSource = dataSource;
        _responder = new Subject<TData>();
        _responder.Subscribe(_dataSource.SetData);
    }

    //public IDataMutation CreateMutate(string name, Func<TData, Task<TData?>> transform, object? hash = null)
    //{
    //    async Task Runner()
    //    {
    //        var subject = new Subject<TData>();

    //        _responder.Push(await transform(_dataSource.GetData()));
    //    }

    //    return new AsyncDataMutation<TData>(Runner, name, hash);
    //}

    public IEventSource<TRespond> EventSource<TRespond>(
        Func<TData, TRespond> transformer,
        Func<TData, bool>? where = null)
        => new EventSource<TRespond, TData>(transformer, where, _responder);

    public IEventSource<TRespond> EventSource<TRespond>(Func<IObservable<TData>, IObservable<TRespond>> transform)
        => new EventSource<TRespond, TData>(transform(_responder.AsObservable()));


    public void Mutate(string name, Func<IObservable<TData>, IObservable<TData?>> transform, object? hash = null)
        => Mutate(CreateMutate(name, transform, hash));

    public IDataMutation CreateMutate(
        string name, Func<IObservable<TData>, IObservable<TData?>> transform,
        object? hash = null)
    {
        void Runner()
        {
            using var sender = new Subject<TData>();

            transform(sender.AsObservable()).NotNull().Subscribe(m => _responder.OnNext(m));
            sender.OnNext(_dataSource.GetData());
            sender.OnCompleted();
            sender.Dispose();
        }

        return new DataMutation<TData>(Runner, name, hash);
    }
}

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

            var data = await _dataSource.GetData(query);

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
                            await _root(query, data);
                            _handler.OnNext(data);
                        }
                        finally
                        {
                            await _completer(query);
                        }

                        return Unit.Default;
                    })
               .Timeout(TimeSpan.FromMinutes(10));

            if (onCompled is null)
                handler.Subscribe();
            else
                handler.SubscribeSafe(onCompled);
        }
    }
}

[PublicAPI]
public class MutatingEngine
{
    protected Action<IDataMutation> Mutator { get; }
    public IDriverFactory DriverFactory { get; }

    protected MutatingEngine(Action<IDataMutation> mutator, IDriverFactory driverFactory)
    {
        Mutator = mutator;
        DriverFactory = driverFactory;
    }

    public static MutatingEngine Create(IDriverFactory driverFactory) 
        => new(driverFactory.CreateMutator(), driverFactory);

    public static ExtendedMutatingEngine<TData> From<TData>(IExtendedDataSource<TData> source, IDriverFactory factory) 
        where TData : class 
        => new(factory.CreateMutator(), source, factory);

    public static ExtendedMutatingEngine<TData> From<TData>(IExtendedDataSource<TData> source, MutatingEngine parent)
        where TData : class
        => new(parent.Mutator, source, parent.DriverFactory);

    public static MutatingEngine<TData> From<TData>(IDataSource<TData> source, IDriverFactory factory) 
        where TData : class 
        => new(factory.CreateMutator(), source, factory);

    public static MutatingEngine<TData> From<TData>(IDataSource<TData> source, MutatingEngine parent)
        where TData : class
        => new(parent.Mutator, source, parent.DriverFactory);

    public static MutatingEngine<TData> Dummy<TData>(IDataSource<TData> source)
        where TData : class
        => new(source);

    public void Mutate(IDataMutation mutationOld) => Mutator(mutationOld);
}

[PublicAPI]
public static class MutatinEngineExtensions
{
    public static IEventSource<TEvent> EventSource<TData, TEvent>(this IEventSourceable<MutatingContext<TData>> engine)
        where TEvent : MutatingChange
    {
        return engine.EventSource(c => c.GetChange<TEvent>(), c => c.Change?.Cast<TEvent>() != null);
    }
}