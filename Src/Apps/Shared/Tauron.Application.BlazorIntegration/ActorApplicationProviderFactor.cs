using System;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
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
        public IActorApplicationBuilder CreateBuilder(IServiceCollection services)
        {
            services.Remove(services.Single(d => d.ImplementationType == typeof(EventLogLoggerProvider)));
            services.Remove(services.Single(d => d.ImplementationType == typeof(ConsoleLoggerProvider)));

            return ActorApplication.Create(services.AddLogging(lb => lb.AddProvider(new NLogLoggerProvider())));
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
            var lifeTime = ActorApplication.Application.Continer.Resolve<IAspLifetime>();

            lifeTime.ApplicationStopping.Register(() =>
                                                  {
                                                      if(!task.IsCompleted)
                                                          ActorApplication.Application.Continer.Resolve<IApplicationLifetime>().Shutdown(0);
                                                  });
            task.ContinueWith(_ => lifeTime.StopApplication());
            
            return new AutofacServiceProvider(ActorApplication.Application.Continer);
        }
    }
}