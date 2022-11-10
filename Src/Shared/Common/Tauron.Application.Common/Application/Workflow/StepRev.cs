using System.Collections.Generic;
using System.Text;

namespace Tauron.Application.Workflow;

public sealed class StepRev<TState, TContext>
{
    public StepRev(TState step)
    {
        Step = step;
        Conditions = new List<ICondition<TContext>>();
    }

    public TState Step { get; }

    public IList<ICondition<TContext>> Conditions { get; }

    public ICondition<TContext>? GenericCondition { get; set; }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(Step);

        foreach (var condition in Conditions) stringBuilder.AppendLine($"->{condition};");

        if(GenericCondition != null) stringBuilder.Append($"Generic->{GenericCondition};");

        return stringBuilder.ToString();
    }
}