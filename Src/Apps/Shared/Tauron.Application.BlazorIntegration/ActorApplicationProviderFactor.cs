using System;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using Tauron.Host;
using IAspLifetime = Microsoft.Extensions.Hosting.IHostApplicationLifetime;

namespace Tauron.Application.AspIntegration
{
    public sealed class ActorApplicationProviderFactory : IServiceProviderFactory<IActorApplicationBuilder>
    {
        private readonly Action<IActorApplicationBuilder>? _configurate;

        public ActorApplicationProviderFactory(Action<IActorApplicationBuilder>? configurate = null)
        {
            _configurate = configurate;
        }

        public IActorApplicationBuilder CreateBuilder(IServiceCollection services)
        {
            services.Remove(services.Single(d => d.ImplementationType == typeof(EventLogLoggerProvider)));
            services.Remove(services.Single(d => d.ImplementationType == typeof(ConsoleLoggerProvider)));

            var builder = ActorApplication.Create(services.AddLogging(lb => lb.AddProvider(new NLogLoggerProvider())));
            _configurate?.Invoke(builder);

            return builder;
        }

        public IServiceProvider CreateServiceProvider(IActorApplicationBuilder containerBuilder)
        {
            var task = containerBuilder
                      .ConfigureAutoFac(cb =>
                                        {
                                            cb.Register(c => new EventLogLoggerProvider(c.Resolve<IOptions<EventLogSettings>>())).As<ILoggerProvider>()
                                              .SingleInstance();
                                        })
                      .Build(ConfigurationOptions.ResolveFromContainer).Run();
            var lifeTime = ActorApplication.Application.Container.Resolve<IAspLifetime>();

            lifeTime.ApplicationStopping.Register(() =>
                                                  {
                                                      if(!task.IsCompleted)
                                                          ActorApplication.Application.Container.Resolve<IApplicationLifetime>().Shutdown(0);
                                                  });
            task.ContinueWith(_ => lifeTime.StopApplication());
            
            return new AutofacServiceProvider(ActorApplication.Application.Container);
        }
    }
}