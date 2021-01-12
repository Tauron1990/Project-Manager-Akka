using JetBrains.Annotations;

namespace Tauron.Application.AkkNode.Services.CommandsOld
{
    [PublicAPI]
    public sealed class FileTransactionId
    {
        public string Id { get; }

        public FileTransactionId(string id) => Id = id;

    }
}