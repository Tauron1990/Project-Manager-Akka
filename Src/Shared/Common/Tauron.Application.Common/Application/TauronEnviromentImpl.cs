

using System.Collections.Generic;
using Zio;
using Zio.FileSystems;

namespace Tauron.Application;

public sealed class TauronEnviromentImpl : ITauronEnviroment
{
    private DirectoryEntry? _defaultPath;

    public TauronEnviromentImpl()
    {
        var fileSystem = new PhysicalFileSystem();
        
        LocalApplicationData = fileSystem.GetDirectoryEntry(TauronEnviroment.LocalApplicationData);
        LocalApplicationTempFolder = fileSystem.GetDirectoryEntry(TauronEnviroment.LocalApplicationTempFolder);
    }

    DirectoryEntry ITauronEnviroment.DefaultProfilePath
    {
        get => _defaultPath ??= TauronEnviroment.DefaultPath.Value;
    }

    public DirectoryEntry LocalApplicationData { get; }

    public DirectoryEntry LocalApplicationTempFolder { get; }

    public IEnumerable<DirectoryEntry> GetProfiles(string application)
        => TauronEnviroment.DefaultProfilePath.EnumerateDirectories();
}