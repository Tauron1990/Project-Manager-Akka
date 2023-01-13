using System.Threading;

namespace Tauron.Application;

public sealed record Context<TContext>(TContext Data, ContextMetadata Metadata, CancellationToken Token)
{
    public void Deconstruct(out ContextMetadata? contextMetadata, out CancellationToken cancellationToken)
    {
        contextMetadata = Metadata;
        cancellationToken = Token;
    }
}