using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Newtonsoft.Json;
using NLog;
using Tauron.Application.AkkaNode.Bootstrap.IpcMessages;
using Tauron.Servicemnager.Networking;
using Tauron.Servicemnager.Networking.Data;
using Tauron.Servicemnager.Networking.IPC;

namespace Tauron.Application.AkkaNode.Bootstrap;

internal sealed class IpcConnection : IIpcConnection, IDisposable
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
                    if(masterExists)
                    {
                        IsReady = false;
                        ErrorMessage = "Duplicate Server Start";

                        return;
                    }

                    _dataServer = new SharmServer(AppNode.IpcName, errorHandler);
                    _dataServer.OnMessageReceived += (_, args) => _messageHandler.OnNext(args.Message);

                    break;
                case IpcApplicationType.Client:
                    if(!masterExists)
                    {
                        IsReady = false;
                        ErrorMessage = "No Server Found";

                        return;
                    }

                    _dataClient = new SharmClient(AppNode.IpcName, errorHandler);
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
        if(!IsReady)
            return Observable.Empty<CallResult<TType>>();

        string type = typeof(TType).AssemblyQualifiedName ??
                      throw new InvalidOperationException("Invalid Message Type");

        return _messageHandler
           .Where(nm => string.Equals(nm.Type, type, StringComparison.Ordinal))
           .SelectSafe(nm => JsonConvert.DeserializeObject<TType>(Encoding.UTF8.GetString(nm.Data))!);
    }

    public bool SendMessage<TMessage>(in Client to, TMessage message)
    {
        if(!IsReady)
            return false;

        string name = typeof(TMessage).AssemblyQualifiedName ??
                      throw new InvalidOperationException("Invalid Message Type");
        string data = JsonConvert.SerializeObject(message);

        var nm = NetworkMessage.Create(name, Encoding.UTF8.GetBytes(data));

        if(_dataClient != null)
            return _dataClient.Send(nm);

        return _dataServer != null && _dataServer.Send(to, nm);
    }

    public bool SendMessage<TMessage>(TMessage message) => SendMessage(Client.All, message);

    internal void Start(string serviceName)
    {
        try
        {
            _dataServer?.Start();

            if(_dataClient is null) return;

            _dataClient.Connect();

            if(SendMessage(new RegisterNewClient(SharmComunicator.ProcessId, serviceName)))
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
        if(_dataClient is SharmClient client)
            client.Disconnect();
    }
}