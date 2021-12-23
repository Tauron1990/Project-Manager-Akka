using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using JetBrains.Annotations;
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

[PublicAPI]
public static class TauronEnviroment
{
    internal static string AppRepository = "Tauron";

    internal static Lazy<IDirectory> DefaultPath = new(
        () =>
        {
            var defaultPath = LocalApplicationData;
            return new VirtualFileFactory().Local(defaultPath);
        },
        LazyThreadSafetyMode.ExecutionAndPublication);

    internal static IDirectory DefaultProfilePath => DefaultPath.Value;

    internal static string LocalApplicationData => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppRepository);

    internal static string LocalApplicationTempFolder => Path.Combine(LocalApplicationData, "Temp");
}