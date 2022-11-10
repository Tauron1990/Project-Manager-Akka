using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Tauron.Application;

[PublicAPI]
public abstract class SimpleTransaction<TContext>
{
    private ImmutableList<InternalStep> _registrations = ImmutableList<InternalStep>.Empty;

    protected void Register(RunStep<TContext> runner)
        => _registrations = _registrations.Add(async (context, roll) => roll.Push(await runner(context).ConfigureAwait(false)));

    protected void RegisterLoop<TItem>(Func<Context<TContext>, IEnumerable<TItem>> enumSelector, RunLoogItem<TContext, TItem> runner)
        => _registrations = _registrations.Add(
            async (context, roll) =>
            {
                foreach (TItem item in enumSelector(context))
                    roll.Push(await runner(context, item).ConfigureAwait(false));
            });

    public async ValueTask<TransactionResult> Execute(TContext contextData, CancellationToken token = default)
    {
        var rollback = new Stack<Rollback<TContext>>();
        var context = new Context<TContext>(contextData, new ContextMetadata(), token);

        try
        {
            foreach (InternalStep step in _registrations)
                await step(context, rollback).ConfigureAwait(false);

            return new TransactionResult(TrasnactionState.Successeded, Exception: null);
        }
        catch (Exception exception)
        {
            try
            {
                while (rollback.Count != 0)
                    await rollback.Pop()(context).ConfigureAwait(false);

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