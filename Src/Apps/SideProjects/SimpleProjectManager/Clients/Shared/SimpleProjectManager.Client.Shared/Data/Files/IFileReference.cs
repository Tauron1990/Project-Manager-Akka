using System.IO;
using System.Threading;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Client.Shared.Data.Files;

public interface IFileReference
{
    FileName Name { get; }

    FileMime ContentType { get; }

    FileSize Size { get; }

    Stream OpenReadStream(in MaxSize maxSize, CancellationToken token);
}