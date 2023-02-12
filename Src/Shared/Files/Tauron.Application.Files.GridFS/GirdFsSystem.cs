using System;
using JetBrains.Annotations;
using Tauron.Application.VirtualFiles;
using Tauron.Application.VirtualFiles.Core;

namespace Tauron.Application.Files.GridFS;

[PublicAPI]
public sealed class GirdFsSystem : DelegatingVirtualFileSystem<GridFsDirectory>
{
    public static readonly string Scheme = "Mongo::";
    public GirdFsSystem(GridFsDirectory context) 
        : base(context, FileSystemFeature.None)
    {
    }

    public override PathInfo Source => Context.OriginalPath;
}