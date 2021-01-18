using Autofac;
using JetBrains.Annotations;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.Master.Commands;
using Tauron.Host;

// ReSharper disable once CheckNamespace
namespace Tauron.Application.AkkaNode.Bootstrap
{
    public static partial class Bootstrap
    {
        [PublicAPI]
        public static IApplicationBuilder StartNode(string[] args, KillRecpientType type)
        {
            //Assemblys.WireUp();
            return ActorApplication.Create(args)
                                   .ConfigureAutoFac(cb =>
                                                     {
                                                         cb.RegisterType<ConsoleAppRoute>().Named<IAppRoute>("default");
                                                         cb.RegisterType<KillHelper>().As<IStartUpAction>();
                                                     })
                                   .ConfigurateNode()
                                   .ConfigureLogging((context, configuration) =>
                                                     {
                                                         System.Console.Title = context.HostEnvironment.ApplicationName;

                                                         configuration.WriteTo.Console(theme:AnsiConsoleTheme.Code);
                                                     })
                                   .ConfigurateAkkaSystem((_, system) =>
                                                          {
                                                              if (type == KillRecpientType.Seed)
                                                                  KillSwitch.Setup(system);
                                                              else
                                                                  KillSwitch.Subscribe(system, type);
                                                          });
        }
    }
}