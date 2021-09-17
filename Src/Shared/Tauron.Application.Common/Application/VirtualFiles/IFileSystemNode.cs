using System;
using JetBrains.Annotations;

namespace Tauron.Application.VirtualFiles
{
    [PublicAPI]
    public interface IFileSystemNode
    {
        string OriginalPath { get; }

        DateTime LastModified { get; }

        IDirectory? ParentDirectory { get; }

        bool IsDirectory { get; }

        bool Exist { get; }

        string Name { get; }

        void Delete();
    }
}