using System;
using JetBrains.Annotations;

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

    void Delete();
}