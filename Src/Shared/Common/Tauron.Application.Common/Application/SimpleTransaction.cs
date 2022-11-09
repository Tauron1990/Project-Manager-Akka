using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Tauron.Application;

[PublicAPI]
public sealed class ContextMetadata
{
    private const string DefaultKey = "Key";

    private readonly ConcurrentDictionary<CacheKey, object?> _meta = new();

    public TData Get<TData>(string name = DefaultKey)
    {
        if(_meta.TryGetValue(new CacheKey(name, typeof(TData)), out object? dat) && dat is TData data)
            return data;

        throw new InvalidOperationException($"Not Metadata Found {name} -- {typeof(TData)}");
    }

    public TData? GetOptional<TData>(string name = DefaultKey)
    {
        if(_meta.TryGetValue(new CacheKey(name, typeof(TData)), out object? dat) && dat is TData data)
            return data;

        return default;
    }

    public void Set<TData>(string name, TData data)
        => _meta[new CacheKey(name, typeof(TData))] = data;

    public void Set<TData>(TData data)
        => _meta[new CacheKey(DefaultKey, typeof(TData))] = data;


    private sealed record CacheKey(string Name, Type Type);
}

public enum TrasnactionState
{
    Successeded,
    Rollback,
    RollbackFailed
}

public sealed record TransactionResult(TrasnactionState State, Exception? Exception);

public sealed record Context<TContext>(TContext Data, ContextMetadata Metadata, CancellationToken Token);

public delegate ValueTask Rollback<TContext>(Context<TContext> context);

public delegate ValueTask<Rollback<TContext>> RunStep<TContext>(Context<TContext> context);

public delegate ValueTask<Rollback<TContext>> RunLoogItem<TContext, in TItem>(Context<TContext> context, TItem item);

[PublicAPI]
public abstract class SimpleTransaction<TContext>
{
    private ImmutableList<InternalStep> _registrations = ImmutableList<InternalStep>.Empty;

    protected void Register(RunStep<TContext> runner)
        => _registrations = _registrations.Add(async (context, roll) => roll.Push(await runner(context)));

    protected void RegisterLoop<TItem>(Func<Context<TContext>, IEnumerable<TItem>> enumSelector, RunLoogItem<TContext, TItem> runner)
        => _registrations = _registrations.Add(
            async (context, roll) =>
            {
                foreach (TItem item in enumSelector(context))
                    roll.Push(await runner(context, item));
            });

    public async ValueTask<TransactionResult> Execute(TContext contextData, CancellationToken token = default)
    {
        var rollback = new Stack<Rollback<TContext>>();
        var context = new Context<TContext>(contextData, new ContextMetadata(), token);

        try
        {
            foreach (InternalStep step in _registrations)
                await step(context, rollback);

            return new TransactionResult(TrasnactionState.Successeded, null);
        }
        catch (Exception exception)
        {
            try
            {
                while (rollback.Count != 0)
                    await rollback.Pop()(context);

                return new TransactionResult(TrasnactionState.Rollback, exception);
            }
            catch (Exception rollbackException)
            {
                return new TransactionResult(TrasnactionState.RollbackFailed, rollbackException);
            }
        }
    }

    private delegate Task InternalStep(Context<TContext> context, Stack<Rollback<TContext>> rollback);
}