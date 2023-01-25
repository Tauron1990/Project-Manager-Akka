using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using Stl.Channels;
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
    private Task _messageHandlerTask = Task.CompletedTask;
    
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
                    _messageHandlerTask = _dataClient.OnMessageReceived.ToAsyncEnumerable()
                       .ForEachAsync(_messageHandler.OnNext);

                    break;
                case IpcApplicationType.NoIpc:
                    IsReady = false;
                    ErrorMessage = "Ipc Disabled";

                    break;
                default:
                    #pragma warning disable EX006
                    throw new ArgumentOutOfRangeException(nameof(type), type, message: null);
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
        _dataClient?.Dispose();
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

    public async ValueTask<bool> SendMessage<TMessage>(Client to, TMessage message)
    {
        if(!IsReady)
            return false;

        string name = typeof(TMessage).AssemblyQualifiedName ??
                      throw new InvalidOperationException("Invalid Message Type");
        string data = JsonConvert.SerializeObject(message);

        var nm = NetworkMessage.Create(name, Encoding.UTF8.GetBytes(data));

        if(_dataClient != null)
            return _dataClient.Send(nm);

        return _dataServer != null && await _dataServer.Send(to, nm).ConfigureAwait(false);
    }

    public ValueTask<bool> SendMessage<TMessage>(TMessage message) => SendMessage(Client.All, message);

    internal async Task Start(string serviceName)
    {
        try
        {
            _dataServer?.Start();

            if(_dataClient is null) return;

            await _dataClient.Run(default).ConfigureAwait(false);
            
            if(await SendMessage(new RegisterNewClient(SharmComunicator.ProcessId, serviceName)).ConfigureAwait(false))
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
            client.Close();
    }
}