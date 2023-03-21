using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Zio;

namespace Tauron.Application;

[PublicAPI]
public static class TauronEnviroment
{
    private static IServiceProvider? _serviceProvider;

    internal static string AppRepository = "Tauron";

    internal static Lazy<DirectoryEntry> DefaultPath = new(
        () =>
        {
            UPath defaultPath = LocalApplicationData;

            return new Zio.FileSystems.PhysicalFileSystem().GetDirectoryEntry(defaultPath);
        },
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static ILoggerFactory LoggerFactory { get; internal set; } = new NullLoggerFactory();

    public static IServiceProvider ServiceProvider
    {
        get
        {
            if(_serviceProvider is null)
                throw new InvalidOperationException("An ServiceProvider was not set");

            return _serviceProvider;
        }
        internal set
        {
            _serviceProvider = value;
            LoggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
        }
    }

    public static DirectoryEntry DefaultProfilePath => DefaultPath.Value;

    public static UPath LocalApplicationData => 
        (UPath)Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) / AppRepository;

    internal static UPath LocalApplicationTempFolder => LocalApplicationData / "Temp";

    public static ILogger GetLogger(Type log)
        => LoggerFactory.CreateLogger(log);

    public static ILogger<TClass> GetLogger<TClass>(TClass marker)
        => LoggerFactory.CreateLogger<TClass>();

    public static ILogger<TClass> GetLogger<TClass>()
        => LoggerFactory.CreateLogger<TClass>();
}