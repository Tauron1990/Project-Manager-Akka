using JetBrains.Annotations;

namespace Tauron.Application.ActorWorkflow
{
    [PublicAPI]
    #pragma warning disable AV1564
    public sealed record WorkflowResult<TContext>(bool Succesfully, string Error, TContext Context);
    #pragma warning restore AV1564
}