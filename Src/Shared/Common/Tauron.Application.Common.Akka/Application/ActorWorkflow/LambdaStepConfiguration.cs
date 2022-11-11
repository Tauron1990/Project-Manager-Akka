using JetBrains.Annotations;
using Tauron.Application.Workflow;

namespace Tauron.Application.ActorWorkflow;

[PublicAPI]
public sealed class LambdaStepConfiguration<TContext>
{
    private Func<TContext, LambdaStep<TContext>, StepId>? _onExecute;
    private Action<TContext>? _onFinish;
    private Func<TContext, LambdaStep<TContext>, StepId>? _onNextElement;
    private TimeSpan? _timeout;

    public void OnExecute(Func<TContext, LambdaStep<TContext>, StepId> func)
    {
        _onExecute = _onExecute.Combine(func);
    }

    public void OnNextElement(Func<TContext, LambdaStep<TContext>, StepId> func)
    {
        _onNextElement = _onNextElement.Combine(func);
    }

    public void OnExecute(Func<TContext, StepId> func)
        => _onExecute = _onExecute.Combine((context, _) => func(context));

    public void OnNextElement(Func<TContext, StepId> func)
        => _onNextElement = _onNextElement.Combine((context, _) => func(context));

    public void OnFinish(Action<TContext> func)
        => _onFinish = _onFinish.Combine(func);

    public void WithTimeout(TimeSpan timeout)
        => _timeout = timeout;


    public LambdaStep<TContext> Build() => new(_onExecute, _onNextElement, _onFinish, _timeout);
}