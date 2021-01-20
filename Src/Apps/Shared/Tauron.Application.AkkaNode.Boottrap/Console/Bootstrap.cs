using System.Threading;
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
        public const string IpcName = "Project_Manager_{{A9A782E5-4F9A-46E4-8A71-76BCF1ABA748}}";

        [PublicAPI]
        public static IApplicationBuilder StartNode(string[] args, KillRecpientType type, IpcApplicationType ipcType)
        {iz
            var masterReady = false;
            if (ipcType != IpcApplicationType.NoIpc)
                masterReady = MasterIpcReady();

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

        private static bool MasterIpcReady()
        {
            try
            {
                using var mt = new Mutex(true, "Global\\" + IpcName + "SharmNet_MasterMutex");
                if (!mt.WaitOne(500)) return true;
                
                mt.ReleaseMutex();
                return false;

            }
            catch (AbandonedMutexException)
            {
                return false;
            }
        }
    }
}