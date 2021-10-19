﻿using System;
using System.IO;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.LocalVirtualFileSystem;

namespace Tauron.Application.VirtualFiles.Resolvers;

public sealed class LocalFileSystemResolver : IFileSystemResolver
{
    public const string SchemeName = "local";

    public string Scheme => SchemeName;

    public IVirtualFileSystem? TryResolve(PathInfo path, IServiceProvider services)
    {
        var relative = GenericPathHelper.ToRelativePath(path);
        var fullQualifed = Path.GetFullPath(relative.Path);

        return Directory.Exists(fullQualifed) ? new LocalSystem(fullQualifed) : null;
    }
}