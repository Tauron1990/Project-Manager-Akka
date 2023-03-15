using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using Stl;
using Tauron.Application.Workshop.Analyzing;
using Tauron.Application.Workshop.Driver;
using Tauron.Operations;

namespace Tauron.Application.Workshop.Mutation;

[PublicAPI]
public sealed class MutatingEngine<TData> : MutatingEngine, IEventSourceable<TData>
    where TData : class
{
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
        : base(_ => { }, new DummyFactory())
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

    private class DummyFactory : IDriverFactory
    {
        public Action<IDataMutation> CreateMutator()
            => _ => { };

        public Action<RegisterRule<TWorkspace, TData1>> CreateAnalyser<TWorkspace, TData1>(TWorkspace workspace, IObserver<RuleIssuesChanged<TWorkspace, TData1>> observer) where TWorkspace : WorkspaceBase<TData1> where TData1 : class
            => _ => { };

        public void CreateProcessor(Type processor, string name) { }

        public Option<Action<IOperationResult>> GetResultSender()
            => Option<Action<IOperationResult>>.None;
    }
}

[PublicAPI]
public class MutatingEngine
{
    protected MutatingEngine(Action<IDataMutation> mutator, IDriverFactory driverFactory)
    {
        Mutator = mutator;
        DriverFactory = driverFactory;
    }

    protected Action<IDataMutation> Mutator { get; }
    public IDriverFactory DriverFactory { get; }

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