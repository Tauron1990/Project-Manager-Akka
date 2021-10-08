using System;

namespace Tauron.Application.VirtualFiles;

[Flags]
public enum FileSystemFeature
{
    None = 0,
    Write = 1 << 0,
    Read = 1 << 1,
    Save = 1 << 2,
    RealTime = 1 << 3,
    Moveable = 1 << 4,
    Delete = 1 << 6,
    Extension = 1 << 7,
    Create = 1 << 8,
    Reloading = 1 << 9
}