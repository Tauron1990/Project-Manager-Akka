using JetBrains.Annotations;

namespace Tauron.Application.AkkaNode.Services.Commands
{
    [PublicAPI]
    public sealed record FileTransactionId(string Id);
}