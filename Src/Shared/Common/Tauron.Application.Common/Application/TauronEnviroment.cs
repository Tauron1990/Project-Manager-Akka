using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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

public class TauronEnviromentSetup
{
    public void Run(IServiceProvider provider)
    {
        TauronEnviroment.ServiceProvider = provider;
    }
}

[PublicAPI]
public static class TauronEnviroment
{
    private static IServiceProvider? _serviceProvider;
    public static ILoggerFactory LoggerFactory { get; internal set; } = new NullLoggerFactory();

    public static IServiceProvider ServiceProvider
    {
        get
        {
            if (_serviceProvider is null)
                throw new InvalidOperationException("An ServiceProvider was not set");

            return _serviceProvider;
        }
        internal set
        {
            _serviceProvider = value;
            LoggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
        }
    }

    public static ILogger GetLogger(Type log)
        => LoggerFactory.CreateLogger(log);

    public static ILogger<TClass> GetLogger<TClass>(TClass marker)
        => LoggerFactory.CreateLogger<TClass>();

    public static ILogger<TClass> GetLogger<TClass>()
        => LoggerFactory.CreateLogger<TClass>();

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