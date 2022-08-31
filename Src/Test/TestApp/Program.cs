using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using LiteDB.Async;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleProjectManager.Client.Shared.Data.States;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Applicarion.Redux.Extensions.Cache;

namespace TestApp;

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
        await Task.Delay(1000, t);

        return _counter;
    }

    public Task<string?> Increment(IncrementCommand command, CancellationToken t)
    {
        Interlocked.Increment(ref _counter);
        using (Computed.Invalidate())
            _ = FakeServer(default);

        return Task.FromResult<string?>(string.Empty);
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
        : this(0) {}
}

public sealed class CounterState : StateBase<TestCounter>
{
    private readonly IFakeServer _fakeServer;
    public CounterState(IFakeServer fakeServer, IStateFactory stateFactory) : base(stateFactory)
        => _fakeServer = fakeServer;

    protected override IStateConfiguration<TestCounter> ConfigurateState(ISourceConfiguration<TestCounter> configuration)
        => configuration.FromCacheAndServer(_fakeServer.FakeServer, (_, i) => new TestCounter(Count: i))
           .ApplyRequests(f => f.AddRequest<IncrementCommand>(_fakeServer.Increment, (counter, _) => new TestCounter(counter.Count + 1)));

    public IObservable<int> ActualCount { get; private set; } = Observable.Empty<int>();

    protected override void PostConfiguration(IRootStoreState<TestCounter> state)
    {
        base.PostConfiguration(state);
        ActualCount = state.Select(tc => tc.Count);
    }
}

static class Program
{
    private static void Test()
    {
        dynamic test = new ExpandoObject();
        test.Hallo = "Test";
        test.Hallo2 = new ExpandoObject();
        test.Hallo2.Hallo3 = "Test2";
    }
    
    private static async Task<ILiteDatabaseAsync> Test(ILiteDatabaseAsync dbTest)
    {
        using var db = await dbTest.BeginTransactionAsync();
        var coll = db.GetCollection<CriticalError>();
        await coll.InsertAsync(new CriticalError("TestError", DateTime.Now, "TestPart", "TestMessage", new StackTrace().ToString(), ImmutableList<ErrorProperty>.Empty));
        await db.CommitAsync();

        return db;
    }

    public sealed record TestData(int Number, string StringData, DateTime Time, bool TestBool, double TestDouble, ImmutableList<TestData> TestList);

    public static BsonValue MapBson(JToken? token)
        => token switch
        {
            null => BsonValue.Null,
            JArray array => new BsonArray(array.Select(MapBson)),
            JObject jObject => new BsonDocument(jObject.ToDictionary<KeyValuePair<string, JToken?>, string, BsonValue>(p => p.Key, p => MapBson(p.Value))),
            JValue jValue => new BsonValue(jValue.Value),
            _ => throw new InvalidOperationException($"Token tyoe not Supported: {token.GetType()}")
        };

    public static JToken? MapJToken(BsonValue value)
        => value switch
        {
            BsonArray array => new JArray(array.Select(MapJToken)),
            BsonDocument document => new JObject(document.Select(p => new JProperty(p.Key, MapJToken(p.Value)))),
            _ => value.Type switch
            {
                BsonType.Null => null,
                BsonType.Int32 => new JValue(value.AsInt32),
                BsonType.Int64 => new JValue(value.AsInt64),
                BsonType.Double => new JValue(value.AsDouble),
                BsonType.Decimal => new JValue(value.AsDecimal),
                BsonType.String => new JValue(value.AsString),
                BsonType.Binary => new JValue(value.AsBinary),
                BsonType.ObjectId => new JValue(value.AsObjectId.ToString()),
                BsonType.Guid => new JValue(value.AsGuid),
                BsonType.Boolean => new JValue(value.AsBoolean),
                BsonType.DateTime => new JValue(value.AsDateTime),
                _ => throw new ArgumentOutOfRangeException(nameof(value), $"BsonValue type not Supported: {value.Type}")
            }
        };
    
    static async Task Main()
    {
        var testData = new TestData(
            1,
            "Hallo",
            DateTime.Now,
            true,
            1.5d,
            ImmutableList<TestData>.Empty
               .Add(new TestData(1, "Hallo1", DateTime.Now - TimeSpan.FromHours(5), false, 2.5d, ImmutableList<TestData>.Empty))
               .Add(new TestData(2, "Hallo2", DateTime.Now - TimeSpan.FromDays(5), true, 3.5d, ImmutableList<TestData>.Empty
                   .Add(new TestData(3, "Test3", DateTime.Now - TimeSpan.FromDays(100), false, 4.5d, ImmutableList<TestData>.Empty)))));

        var bson = MapBson(JToken.FromObject(testData));
        var token = MapJToken(bson);

        var testData2 = token?.ToObject<TestData>();
        
        var coll = new ServiceCollection();
        coll.AddTransient<ICacheDb, FakeCahce>();
        coll.AddTransient<IErrorHandler, FakeError>();
        coll.AddStoreConfiguration();
        coll.AddFusion().AddComputeService<IFakeServer, FakeServerImpl>();

        await using var serviceProvider = coll.BuildServiceProvider();
        var stateFactory = serviceProvider.GetRequiredService<IStateFactory>();
        var configuration = serviceProvider.GetRequiredService<IStoreConfiguration>();

        var test = CreateState<CounterState>();
        var store = configuration.Build();
        
        test.ActualCount
           .Do(
                c =>
                {
                    if(c != 10)
                        store.Dispatch(new IncrementCommand());
                })
           .Subscribe(Console.WriteLine);

        Console.ReadKey();
        
        TState CreateState<TState>()
        {
            var state = ActivatorUtilities.CreateInstance<TState>(serviceProvider, stateFactory);

            if(state is IStoreInitializer baseState)
                baseState.RunConfig(configuration);

            return state;
        }
    }
}