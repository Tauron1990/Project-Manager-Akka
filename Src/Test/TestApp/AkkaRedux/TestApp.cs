/*using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka;
using Akka.Hosting;
using Akka.Streams;
using Akka.Streams.Dsl;
using Microsoft.Extensions.DependencyInjection;
using Stl.Fusion;
using Tauron;
using Tauron.Application.Akka.Redux;
using Tauron.Application.Akka.Redux.Configuration;
using Tauron.Application.Akka.Redux.Extensions.Cache;

namespace TestApp.AkkaRedux;

public static class AkkaTestApp
{
    public abstract class StateBase
    {
        protected IStateFactory StateFactory { get; }


        protected StateBase(IStateFactory stateFactory)
            => StateFactory = stateFactory;

        // protected IObservable<TValue> FromServer<TValue>(Func<CancellationToken, Task<TValue>> fetcher)
        //     => Observable.Create<TValue>(
        //         o =>
        //         {
        //             var state = StateFactory.NewComputed<TValue>(async (_, t) => await fetcher(t));
        //
        //             return new CompositeDisposable(state, state.ToObservable(true).Subscribe(o));
        //         });
    }

    public interface IStoreInitializer
    {
        void RunConfig(IStoreConfiguration configuration);
    }

    public abstract class StateBase<TState> : StateBase, IStoreInitializer
        where TState : class, new()
    {

        protected Func<object, Task<IQueueOfferResult>> Dispatch { get; private set; } =
            static _ => Task.FromResult<IQueueOfferResult>(QueueOfferResult.Dropped.Instance);

        protected StateBase(IStateFactory stateFactory)
            : base(stateFactory) { }


        void IStoreInitializer.RunConfig(IStoreConfiguration configuration)
        {
            configuration.NewState<TState>(
                s => ConfigurateState(s).AndFinish(
                    store =>
                    {
                        Dispatch = store.Dispatch;
                        PostConfiguration(store.ForState<TState>());
                    }));
        }

        protected abstract IStateConfiguration<TState> ConfigurateState(ISourceConfiguration<TState> configuration);

        protected virtual void PostConfiguration(IRootStoreState<TState> state) { }
    }

    public sealed record IncrementCommand;

    public interface IFakeServer
    {
        [ComputeMethod]
        Task<int> FakeServer(CancellationToken t);

        Task<string?> Increment(IncrementCommand command, CancellationToken t);
    }

    public class FakeServerImpl : IFakeServer
    {
        private int _counter = 1;

        public virtual async Task<int> FakeServer(CancellationToken t)
        {
            await Task.Delay(1000, t).ConfigureAwait(false);

            return _counter;
        }

        public async Task<string?> Increment(IncrementCommand command, CancellationToken t)
        {
            await Task.Delay(1500, t).ConfigureAwait(false);

            Interlocked.Increment(ref _counter);
            using (Computed.Invalidate())
                _ = FakeServer(default);

            return string.Empty;
        }
    }

    public sealed class FakeError : IErrorHandler
    {
        public void RequestError(string error)
            => Console.WriteLine($"Error: {error}");

        public void RequestError(Exception error)
            => RequestError(error.ToString());

        public void StateDbError(Exception error)
            => Console.WriteLine($"State DB Error: {error}");

        public void TimeoutError(Exception error)
            => Console.WriteLine($"Timeout Error: {error}");

        public void StoreError(Exception error)
            => Console.WriteLine($"Store Error: {error}");
    }

    public sealed class FakeCahce : ICacheDb
    {
        public ValueTask DeleteElement(CacheTimeoutId key)
            => default;

        public ValueTask DeleteElement(CacheDataId key)
            => default;

        public ValueTask<(CacheTimeoutId? id, CacheDataId? Key, DateTime Time)> GetNextTimeout()
            => default;

        public ValueTask TryAddOrUpdateElement(CacheDataId key, string data)
            => default;

        public ValueTask<string?> ReNewAndGet(CacheDataId key)
            => default;
    }

    public sealed record TestCounter(int Count)
    {
        public TestCounter()
            : this(0) { }
    }

    public sealed class CounterState : StateBase<TestCounter>
    {
        private readonly IFakeServer _fakeServer;

        public CounterState(IFakeServer fakeServer, IStateFactory stateFactory) : base(stateFactory)
            => _fakeServer = fakeServer;


        protected override IStateConfiguration<TestCounter> ConfigurateState(ISourceConfiguration<TestCounter> configuration)
            => configuration.FromCacheAndServer(_fakeServer.FakeServer, (d, i) => d with { Count = i })
               .ApplyRequests(f => f.AddRequest<IncrementCommand>(_fakeServer.Increment, (counter, _) => counter with { Count = counter.Count + 1 }));

        public Source<int, NotUsed> ActualCount { get; private set; } = Source.Empty<int>();

        protected override void PostConfiguration(IRootStoreState<TestCounter> state)
        {
            base.PostConfiguration(state);
            ActualCount = state.Select(Flow.Create<TestCounter>().Select(tc => tc.Count));
        }
    }

    public static async Task AkkaMain()
    {
        var coll = new ServiceCollection();
        coll.AddAkka("TestAkka", _ => {});
        coll.AddTransient<ICacheDb, FakeCahce>();
        coll.AddTransient<IErrorHandler, FakeError>();
        coll.AddStoreConfiguration();
        coll.AddFusion().AddComputeService<IFakeServer, FakeServerImpl>();

        await using var serviceProvider = coll.BuildServiceProvider();
        var stateFactory = serviceProvider.GetRequiredService<IStateFactory>();
        var configuration = serviceProvider.GetRequiredService<IStoreConfiguration>();

        var test = CreateState<CounterState>();
        var store = configuration.Build();

        var task = test.ActualCount.RunForeach(Console.WriteLine, store.Materializer);
        
        var service = serviceProvider.GetRequiredService<IFakeServer>();
        await service.Increment(new IncrementCommand(), default);

        // for (var i = 0; i < 1; i++)
        // {
        //     store.Dispatch(new IncrementCommand());
        //     await Task.Delay(1000);
        // }


        TState CreateState<TState>()
        {
            var state = ActivatorUtilities.CreateInstance<TState>(serviceProvider, stateFactory);

            if(state is IStoreInitializer baseState)
                baseState.RunConfig(configuration);

            return state;
        }

        Console.ReadKey();
    }
}*/