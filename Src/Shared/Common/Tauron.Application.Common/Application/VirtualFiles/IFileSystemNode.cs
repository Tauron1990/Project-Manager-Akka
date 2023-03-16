using System;

namespace Tauron.Application.VirtualFiles;

[PublicAPI]
public interface IFileSystemNode<TSelf>
    where TSelf : IFileSystemNode<TSelf>
{
    FileSystemFeature Features { get; }

    NodeType Type { get; }

    PathInfo OriginalPath { get; }

    DateTime LastModified { get; }

    IDirectory? ParentDirectory { get; }

    bool Exist { get; }

    string Name { get; }

    [Pure]
    Result<TSelf> Delete();
}