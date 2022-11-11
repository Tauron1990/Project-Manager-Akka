using JetBrains.Annotations;
using Tauron.Application.Workflow;

namespace Tauron.Application.ActorWorkflow;

[PublicAPI]
public abstract class LambdaWorkflowActor<TContext> : WorkflowActorBase<LambdaStep<TContext>, TContext>
    where TContext : IWorkflowContext
{
    protected virtual void WhenStep(
        StepId id, Action<LambdaStepConfiguration<TContext>> config,
        Action<StepConfiguration<LambdaStep<TContext>, TContext>>? con = null)
    {
        var stepConfig = new LambdaStepConfiguration<TContext>();
        config.Invoke(stepConfig);
        var concon = WhenStep(id, stepConfig.Build());
        con?.Invoke(concon);
    }
}