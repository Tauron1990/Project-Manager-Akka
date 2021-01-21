using System;
using System.IO;
using System.IO.Pipes;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Autofac;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Servicemnager.Networking.Data;
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
        {
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

        private sealed class IpcConnection : IIpcConnection, IDisposable
        {
            private readonly Subject<NetworkMessage> _messageHandler = new();

            public string ErrorMessage { get; }

            public bool IsReady { get; }

            public IpcConnection(bool masterExists, IpcApplicationType type)
            {
                switch (type)
                {
                    case IpcApplicationType.Server:
                        break;
                    case IpcApplicationType.Client:
                        break;
                    case IpcApplicationType.NoIpc:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }

            public IObservable<TType> OnMessage<TType>()
            {

            }

            public void Start(string serviceName)
            {

            }

            public void Dispose() => _messageHandler.Dispose();
        }



        [UsedImplicitly]
        private sealed class KillHelper : IStartUpAction
        {
            [UsedImplicitly]
            private static KillHelper? _keeper;

            private readonly ActorSystem _system;
            private readonly ILogger _logger;
            private readonly string? _comHandle;

            public KillHelper(IConfiguration configuration, ActorSystem system)
            {
                _logger = Log.ForContext<KillHelper>();
                _comHandle = configuration["ComHandle"];
                _system = system;

                _keeper = this;
                _system.RegisterOnTermination(() => _keeper = null);
            }

            public void Run()
            {
                Task.Factory.StartNew(() =>
                                      {
                                          if (_keeper == null || string.IsNullOrWhiteSpace(_comHandle))
                                              return;

                                          try
                                          {
                                              using var client = new AnonymousPipeClientStream(PipeDirection.In, _comHandle);
                                              using var reader = new BinaryReader(client);

                                              while (_keeper != null)
                                              {
                                                  var data = reader.ReadString();

                                                  switch (data)
                                                  {
                                                      case "Kill-Node":
                                                          _logger.Information("Reciving Killing Notification");
                                                          _system.Terminate();
                                                          _keeper = null;
                                                          break;
                                                  }
                                              }
                                          }
                                          catch (Exception e)
                                          {
                                              _logger.Error(e, "Error on Setup Service kill Watch");
                                          }
                                      }, TaskCreationOptions.LongRunning);
            }
        }
    }
}