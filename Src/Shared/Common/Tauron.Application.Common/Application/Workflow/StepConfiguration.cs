using System;

namespace Tauron.Application.Workflow;

[PublicAPI]
public class StepConfiguration<TState, TContext>
{
    private readonly StepRev<TState, TContext> _context;

    public StepConfiguration(StepRev<TState, TContext> context) => _context = context;

    public StepConfiguration<TState, TContext> WithCondition(ICondition<TContext> condition)
    {
        _context.Conditions.Add(condition);

        return this;
    }

    public ConditionConfiguration<TState, TContext> WithCondition(Func<TContext, IStep<TContext>, bool>? guard = null)
    {
        var con = new SimpleCondition<TContext> { Guard = guard };

        if(guard != null) return new ConditionConfiguration<TState, TContext>(WithCondition(con), con);

        _context.GenericCondition = con;

        return new ConditionConfiguration<TState, TContext>(this, con);
    }
}