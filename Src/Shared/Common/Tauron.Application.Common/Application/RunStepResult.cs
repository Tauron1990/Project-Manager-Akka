namespace Tauron.Application;

public readonly record struct RunStepResult<TContext>(Context<TContext> Context, Rollback<TContext> Rollback);