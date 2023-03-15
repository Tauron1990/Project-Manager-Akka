using System;
using JetBrains.Annotations;
using Tauron.Operations;

namespace Tauron.Application.VirtualFiles;

[PublicAPI]
public interface IFileSystemNode
{
    FileSystemFeature Features { get; }

    NodeType Type { get; }

    PathInfo OriginalPath { get; }

    DateTime LastModified { get; }

    IDirectory? ParentDirectory { get; }

    bool Exist { get; }

    string Name { get; }

    SimpleResult Delete();
}