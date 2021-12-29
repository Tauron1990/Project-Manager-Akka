using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;
using Servicemnager.Networking;
using Servicemnager.Networking.Data;
using Servicemnager.Networking.IPC;
using Tauron.AkkaHost;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.AkkaNode.Bootstrap.Console.IpcMessages;
using Tauron.Application.Master.Commands.KillSwitch;
using ILogger = NLog.ILogger;

// ReSharper disable once CheckNamespace
namespace Tauron.Application.AkkaNode.Bootstrap;

public static partial class Bootstrap
{
    private const string IpcName = "Project_Manager_{{A9A782E5-4F9A-46E4-8A71-76BCF1ABA748}}";

    [PublicAPI]
    public static IHostBuilder StartNode(string[] args, KillRecpientType type, IpcApplicationType ipcType, Action<IActorApplicationBuilder>? build = null, bool consoleLog = false)
        => StartNode(Host.CreateDefaultBuilder(args), type, ipcType, build, consoleLog);

    [PublicAPI]
    public static IHostBuilder StartNode(this IHostBuilder builder, KillRecpientType type, IpcApplicationType ipcType, Action<IActorApplicationBuilder>? build = null, bool consoleLog = false)
    {
        var masterReady = false;
        if (ipcType != IpcApplicationType.NoIpc)
            masterReady = SharmComunicator.MasterIpcReady(IpcName);
        var ipc = new IpcConnection(
            masterReady,
            ipcType,
            (s, exception) => LogManager.GetCurrentClassLogger().Error(exception, "Ipc Error: {Info}", s));

        return builder.ConfigureLogging(
                (context, configuration) =>
                {
                    System.Console.Title = context.HostingEnvironment.ApplicationName;
                    if (consoleLog)
                        configuration.AddConsole();
                })
           .ConfigurateNode(
                ab =>
                {
                    ab.ConfigureServices(
                            (host, cb) =>
                            {
                                cb.TryAddSingleton(host.Configuration);
                                cb.AddHostedService<NodeAppService>();
                                cb.AddScoped<IStartUpAction, KillHelper>();
                                cb.AddSingleton<IIpcConnection>(ipc);
                            })
                       .ConfigureAkkaSystem(
                            (_, system) =>
                            {
                                switch (type)
                                {
                                    case KillRecpientType.Seed:
                                        KillSwitch.Setup(system);
                                        break;
                                    default:
                                        KillSwitch.Subscribe(system, type);
                                        break;
                                }
                            });

                    build?.Invoke(ab);
                });
    }

    private sealed class IpcConnection : IIpcConnection, IDisposable
    {
        private readonly Subject<NetworkMessage> _messageHandler = new();
        private IDataClient? _dataClient;
        private IDataServer? _dataServer;

        internal IpcConnection(bool masterExists, IpcApplicationType type, Action<string, Exception> errorHandler)
        {
            try
            {
                switch (type)
                {
                    case IpcApplicationType.Server:
                        if (masterExists)
                        {
                            IsReady = false;
                            ErrorMessage = "Duplicate Server Start";

                            return;
                        }

                        _dataServer = new SharmServer(IpcName, errorHandler);
                        _dataServer.OnMessageReceived += (_, args) => _messageHandler.OnNext(args.Message);

                        break;
                    case IpcApplicationType.Client:
                        if (!masterExists)
                        {
                            IsReady = false;
                            ErrorMessage = "No Server Found";

                            return;
                        }

                        _dataClient = new SharmClient(IpcName, errorHandler);
                        _dataClient.OnMessageReceived += (_, args) => _messageHandler.OnNext(args.Message);

                        break;
                    case IpcApplicationType.NoIpc:
                        IsReady = false;
                        ErrorMessage = "Ipc Disabled";

                        break;
                    default:
                        #pragma warning disable EX006
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                    #pragma warning restore EX006
                }
            }
            catch (Exception e)
            {
                errorHandler("Global", e);
                ErrorMessage = e.Message;
                IsReady = false;
            }
        }

        internal string ErrorMessage { get; private set; } = string.Empty;

        public void Dispose()
        {
            _messageHandler.Dispose();
            (_dataClient as IDisposable)?.Dispose();
            _dataServer?.Dispose();

            _dataClient = null;
            _dataServer = null;
        }

        public bool IsReady { get; private set; } = true;

        public IObservable<CallResult<TType>> OnMessage<TType>()
        {
            if (!IsReady)
                return Observable.Empty<CallResult<TType>>();

            string type = typeof(TType).AssemblyQualifiedName ??
                          throw new InvalidOperationException("Invalid Message Type");

            return _messageHandler.Where(nm => nm.Type == type).SelectSafe(nm => JsonConvert.DeserializeObject<TType>(Encoding.UTF8.GetString(nm.Data))!);
        }

        public bool SendMessage<TMessage>(string to, TMessage message)
        {
            if (!IsReady)
                return false;

            var name = typeof(TMessage).AssemblyQualifiedName ??
                       throw new InvalidOperationException("Invalid Message Type");
            var data = JsonConvert.SerializeObject(message);

            var nm = NetworkMessage.Create(name, Encoding.UTF8.GetBytes(data));

            if (_dataClient != null)
                return _dataClient.Send(nm);

            return _dataServer != null && _dataServer.Send(to, nm);
        }

        public bool SendMessage<TMessage>(TMessage message) => SendMessage("All", message);

        internal void Start(string serviceName)
        {
            try
            {
                _dataServer?.Start();

                if (_dataClient is null) return;

                _dataClient.Connect();

                if (SendMessage(new RegisterNewClient(SharmComunicator.ProcessId, serviceName)))
                    return;

                IsReady = false;
                ErrorMessage = "Client Message Registration Fail";

                Dispose();
            }
            catch (Exception e)
            {
                IsReady = false;
                ErrorMessage = e.Message;

                LogManager.GetCurrentClassLogger().Error(e, "Error on Starting Ipc");

                Dispose();
            }
        }

        internal void Disconnect()
        {
            if (_dataClient is SharmClient client)
                client.Disconnect();
        }
    }

    [UsedImplicitly]
    public sealed class KillHelper : IStartUpAction
    {
        [UsedImplicitly]
        #pragma warning disable IDE0052 // Ungelesene private Member entfernen
        private static KillHelper? _keeper;
        #pragma warning restore IDE0052 // Ungelesene private Member entfernen

        private readonly string? _comHandle;
        private readonly IpcConnection _ipcConnection;
        private readonly ILogger _logger;

        private readonly ActorSystem _system;

        public KillHelper(IConfiguration configuration, ActorSystem system, IIpcConnection ipcConnection)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _comHandle = configuration["ComHandle"];
            _system = system;
            _ipcConnection = (IpcConnection)ipcConnection;

            _keeper = this;
            system.RegisterOnTermination(
                () =>
                {
                    _ipcConnection.Disconnect();
                    _keeper = null;
                });
        }

        public void Run()
        {
            if (string.IsNullOrWhiteSpace(_comHandle)) return;

            string? errorToReport = null;
            if (_ipcConnection.IsReady)
            {
                _ipcConnection.Start(_comHandle);
                if (!_ipcConnection.IsReady)
                    errorToReport = _ipcConnection.ErrorMessage;
            }
            else
            {
                errorToReport = _ipcConnection.ErrorMessage;
            }

            if (!string.IsNullOrWhiteSpace(errorToReport))
            {
                _logger.Warn("Error on Start Kill Watch: {Error}", errorToReport);

                return;
            }

            _ipcConnection.OnMessage<KillNode>()
               .Subscribe(
                    _ => _system.Terminate(),
                    exception => _logger.Error(exception, "Error On Killwatch Message Recieve"));
        }
    }
}