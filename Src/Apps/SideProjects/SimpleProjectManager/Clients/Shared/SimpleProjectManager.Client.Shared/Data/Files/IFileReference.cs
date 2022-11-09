using System.IO;
using System.Threading;

namespace SimpleProjectManager.Client.Shared.Data.Files;

public interface IFileReference
{
    string Name { get; }

    string ContentType { get; }

    long Size { get; }

    Stream OpenReadStream(long maxSize, CancellationToken token);
}