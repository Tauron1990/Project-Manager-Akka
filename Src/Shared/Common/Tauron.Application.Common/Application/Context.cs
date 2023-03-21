using System.Threading;

namespace Tauron.Application;

[PublicAPI]
public sealed record Context<TContext>(TContext Data, ContextMetadata Metadata, CancellationToken Token)
{
    public RunStepResult<TContext> WithRollback(Rollback<TContext> rollback) =>
        new(this, rollback);

    public void Deconstruct(out ContextMetadata? contextMetadata, out CancellationToken cancellationToken)
    {
        contextMetadata = Metadata;
        cancellationToken = Token;
    }
}