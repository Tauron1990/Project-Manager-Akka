using JetBrains.Annotations;

namespace Tauron.Application.AkkaNode.Services.Reporting.Commands
{
    [PublicAPI]
    public sealed record FileTransactionId(string Id);
}