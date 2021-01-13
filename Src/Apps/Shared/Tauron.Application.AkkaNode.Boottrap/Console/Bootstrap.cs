using AnyConsole;
using Autofac;
using JetBrains.Annotations;
using Serilog;
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
            var console = new ExtendedConsole();

            console.Configure();

            //Assemblys.WireUp();
            return ActorApplication.Create(args)
                                   .ConfigureAutoFac(cb =>
                                                     {
                                                         cb.RegisterInstance(console).SingleInstance().OnActivated(c => c.Instance.Configure(ConsoleConfiguration.Configuration));
                                                         cb.RegisterType<ConsoleAppRoute>().Named<IAppRoute>("default");
                                                         cb.RegisterType<KillHelper>().As<IStartUpAction>();
                                                     })
                                   .ConfigurateNode()
                                   .ConfigureLogging((context, configuration) =>
                                                     {
                                                         console.Title = context.HostEnvironment.ApplicationName;

                                                         configuration.WriteTo.Sink(new AnyConsoleSink(console));
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