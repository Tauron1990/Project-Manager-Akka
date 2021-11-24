using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Tauron.Application;

public delegate ValueTask Rollback<in TContext>(TContext context);

public delegate ValueTask<Rollback<TContext>> RunStep<TContext>(TContext context);

public delegate ValueTask<Rollback<TContext>> RunLoogItem<TContext, in TItem>(TContext context, TItem item);

public enum TrasnactionState
{
    Successeded,
    Rollback,
    RollbackFailed
}

public sealed record TransactionResult(TrasnactionState State, Exception? Exception);

[PublicAPI]
public abstract class SimpleTransaction<TContext>
{
    private delegate Task InternalStep(TContext context, Stack<Rollback<TContext>> rollback);
    private ImmutableList<InternalStep> _registrations = ImmutableList<InternalStep>.Empty;

    protected void Register(RunStep<TContext> runner)
        => _registrations = _registrations.Add(async (context, roll) => roll.Push(await runner(context)));

    protected void RegisterLoop<TItem>(Func<TContext, IEnumerable<TItem>> enumSelector, RunLoogItem<TContext, TItem> runner)
        => _registrations = _registrations.Add(
            async (context, roll) =>
            {
                foreach (var item in enumSelector(context))
                    roll.Push(await runner(context, item));
            });
    
    public async ValueTask<TransactionResult> Execute(TContext context)
    {
        var rollback = new Stack<Rollback<TContext>>();

        try
        {
            foreach (var step in _registrations) 
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
}