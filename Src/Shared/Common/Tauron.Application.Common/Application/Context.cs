using System.Threading;

namespace Tauron.Application;

public sealed record Context<TContext>(TContext Data, ContextMetadata Metadata, CancellationToken Token);