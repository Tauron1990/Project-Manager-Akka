using System;
using System.Threading.Tasks;
using Akka.Actor;
using Autofac;
using Microsoft.Extensions.Configuration;
using NLog;

namespace Tauron.Host
{
    public sealed class CommonLifetime : IHostLifetime
    {
        private readonly string _appRoute;
        private readonly IComponentContext _factory;
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private IAppRoute? _route;

        public CommonLifetime(IConfiguration configuration, IComponentContext factory)
        {
            _factory = factory;
            _appRoute = configuration.GetValue("route", "default");
        }

        public async Task WaitForStartAsync(ActorSystem actorSystem)
        {
            _logger.Info("Begin Start Application");
            try
            {
                string name = !string.IsNullOrEmpty(_appRoute) ? _appRoute : "default";
                _logger.Info("Try get Route for {RouteName}", name);

                _route = GetRoute(name);
            }
            catch (Exception e)
            {
                _logger.Warn(e, "Error on get Route");
                _route = GetRoute(null);
            }

            await _route.WaitForStartAsync(actorSystem);
            ShutdownTask = _route.ShutdownTask;
        }

        public Task ShutdownTask { get; private set; } = Task.CompletedTask;

        private IAppRoute GetRoute(string? name)
            => name == null ? _factory.Resolve<IAppRoute>() : _factory.ResolveNamed<IAppRoute>(name);
    }
}