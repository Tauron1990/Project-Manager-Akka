using System;
using JetBrains.Annotations;

namespace Tauron.Application.VirtualFiles;

[PublicAPI]
public interface IFileSystemNode
{
    FileSystemFeature Features { get; }

    NodeType Type { get; }

    PathInfo OriginalPath { get; }

    Result<DateTime> LastModified { get; }

    Result<IDirectory> ParentDirectory { get; }

    bool Exist { get; }

    Result<string> Name { get; }

    Result Delete();
}