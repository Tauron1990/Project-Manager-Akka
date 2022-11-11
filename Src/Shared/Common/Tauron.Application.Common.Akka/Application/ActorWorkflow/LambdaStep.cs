using JetBrains.Annotations;
using Tauron.Application.Workflow;

namespace Tauron.Application.ActorWorkflow;

[PublicAPI]
public sealed class LambdaStep<TContext> : IStep<TContext>, IHasTimeout
{
    private readonly Func<TContext, LambdaStep<TContext>, StepId>? _onExecute;
    private readonly Action<TContext>? _onFinish;
    private readonly Func<TContext, LambdaStep<TContext>, StepId>? _onNextElement;


    public LambdaStep(
        Func<TContext,
            LambdaStep<TContext>, StepId>? onExecute = null,
        Func<TContext, LambdaStep<TContext>, StepId>? onNextElement = null,
        Action<TContext>? onFinish = null, TimeSpan? timeout = null)
    {
        Timeout = timeout;
        _onExecute = onExecute;
        _onNextElement = onNextElement;
        _onFinish = onFinish;
    }

    public TimeSpan? Timeout { get; }

    public string ErrorMessage { get; set; } = string.Empty;

    public StepId OnExecute(TContext context)
        => _onExecute?.Invoke(context, this) ?? StepId.None;

    public StepId NextElement(TContext context)
        => _onNextElement?.Invoke(context, this) ?? StepId.None;

    public void OnExecuteFinish(TContext context)
        => _onFinish?.Invoke(context);

    public void SetError(string error)
        => ErrorMessage = error;
}