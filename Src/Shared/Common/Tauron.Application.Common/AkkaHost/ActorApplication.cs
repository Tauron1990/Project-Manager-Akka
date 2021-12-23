using System;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tauron.AkkaHost;

public static class ActorApplication
{
    private static IServiceProvider? _serviceProvider;
    private static ActorSystem? _actorSystem;
    public static ILoggerFactory LoggerFactory { get; internal set; } = new NullLoggerFactory();

    public static IServiceProvider ServiceProvider
    {
        get
        {
            if (_serviceProvider is null)
                throw new InvalidOperationException("An ServiceProvider was not set");

            return _serviceProvider;
        }
        internal set => _serviceProvider = value;
    }

    public static ActorSystem ActorSystem
    {
        get
        {
            if (_actorSystem is null)
                throw new InvalidOperationException("An Actorsystem was not set");

            return _actorSystem;
        }
        internal set => _actorSystem = value;
    }

    public static bool IsStarted => _actorSystem != null;

    public static ILogger GetLogger(Type log)
        => LoggerFactory.CreateLogger(log);

    public static ILogger<TClass> GetLogger<TClass>(TClass marker)
        => LoggerFactory.CreateLogger<TClass>();

    public static ILogger<TClass> GetLogger<TClass>()
        => LoggerFactory.CreateLogger<TClass>();
}