using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
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
    //private Task _messageHandlerTask = Task.CompletedTask;
    
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
                    #pragma warning disable EX002, EX006
                    throw new UnreachableException();
                #pragma warning restore EX002, EX006
            }
        }
        catch (Exception e) when(e is not UnreachableException)
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

    public IObservable<Result<TType>> OnMessage<TType>()
    {
        if(!IsReady)
            return Observable.Empty<Result<TType>>();

        string? type = typeof(TType).AssemblyQualifiedName;
        if(string.IsNullOrWhiteSpace(type))
            return Observable.Return<Result<TType>>(Result.Fail("AssemblyQualifiedName was null or Empty"));

        return
            from nm in _messageHandler
            where string.Equals(nm.Type, type, StringComparison.Ordinal)
            let data = Result.Try(() => JsonConvert.DeserializeObject<TType>(Encoding.UTF8.GetString(nm.Data)))
            select data.Bind(
                deserialized => deserialized is null
                    ? Result.Fail("No Data Deserialized")
                    : Result.Ok(deserialized));
    }

    public async ValueTask<Result> SendMessage<TMessage>(Client to, TMessage message)
    {
        if(!IsReady)
            return Result.Fail(new IpcNotReadyError());

        string? name = typeof(TMessage).AssemblyQualifiedName;
        if(string.IsNullOrWhiteSpace(name))
            return Result.Fail(new MessageTypeError(typeof(TMessage)));
        
        string data = JsonConvert.SerializeObject(message);

        var nm = NetworkMessage.Create(name, Encoding.UTF8.GetBytes(data));

        if(_dataClient is not null)
        {
            var sendResult = Result.Try(() => _dataClient.Send(nm));

            return sendResult.Bind(sendOk => sendOk ? Result.Ok() : Result.Fail(new SendNotSuccessfullError()));
        }

        if(_dataServer is not null)
        {
            var sendResult = await Result.Try(() => _dataServer.Send(to, nm)).ConfigureAwait(false);
         
            return sendResult.Bind(sendOk => sendOk ? Result.Ok() : Result.Fail(new SendNotSuccessfullError()));
        }
        
        return Result.Fail(new NoServerOrClientError());
    }

    public ValueTask<Result> SendMessage<TMessage>(TMessage message) => SendMessage(Client.All, message);

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