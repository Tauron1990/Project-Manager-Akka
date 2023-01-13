using System.Collections.Generic;
using Tauron.Application.VirtualFiles;

namespace Tauron.Application;

public sealed class TauronEnviromentImpl : ITauronEnviroment
{
    private IDirectory? _defaultPath;

    public TauronEnviromentImpl(VirtualFileFactory factory)
    {
        LocalApplicationData = factory.Local(TauronEnviroment.LocalApplicationData);
        LocalApplicationTempFolder = factory.Local(TauronEnviroment.LocalApplicationTempFolder);
    }

    IDirectory ITauronEnviroment.DefaultProfilePath
    {
        get => _defaultPath ??= TauronEnviroment.DefaultPath.Value;
        set => _defaultPath = value;
    }

    public IDirectory LocalApplicationData { get; }

    public IDirectory LocalApplicationTempFolder { get; }

    public IEnumerable<IDirectory> GetProfiles(string application)
        => TauronEnviroment.DefaultProfilePath.GetDirectory(application)
           .Directories;
}