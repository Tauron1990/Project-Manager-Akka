using System.Collections.Generic;
using JetBrains.Annotations;

namespace Tauron.Application.VirtualFiles;

[PublicAPI]
public interface IDirectory : IFileSystemNode
{
    IEnumerable<IDirectory> Directories { get; }

    IEnumerable<IFile> Files { get; }

    IFile GetFile(in PathInfo name);

    IDirectory GetDirectory(in PathInfo name);

    IDirectory MoveTo(in PathInfo location);
}