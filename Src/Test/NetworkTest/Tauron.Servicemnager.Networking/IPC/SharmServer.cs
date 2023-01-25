using Tauron.Servicemnager.Networking.Data;

namespace Tauron.Servicemnager.Networking.IPC;

public sealed class SharmServer : IDataServer
{
    private readonly SharmComunicator _comunicator;

    public SharmServer(SharmProcessId uniqeName, Action<string, Exception> errorHandler)
    {
        _comunicator = new SharmComunicator(uniqeName, errorHandler);
        _comunicator.OnMessage += ComunicatorOnOnMessage;
    }

    public void Dispose() => _comunicator.Dispose();

    public event EventHandler<ClientConnectedArgs>? ClientConnected;

    public event EventHandler<ClientDisconnectedArgs>? ClientDisconnected;

    public event EventHandler<MessageFromClientEventArgs>? OnMessageReceived;

    public Task Start()
    {
        _comunicator.Connect();
        
        return Task.CompletedTask;
    }

    public ValueTask<bool> Send(Client client, NetworkMessage message) 
        => ValueTask.FromResult(_comunicator.Send(message, client));

    private void ComunicatorOnOnMessage(NetworkMessage message, ulong messageId, in Client processId)
    {
        #pragma warning disable GU0011
        if(message.Type == SharmComunicatorMessage.RegisterClient)
        {
            _comunicator.Send(message, processId);
            ClientConnected?.Invoke(this, new ClientConnectedArgs(processId));

        }
        else if(message.Type == SharmComunicatorMessage.UnRegisterClient)
        {
            _comunicator.Send(message, processId);
            ClientDisconnected?.Invoke(this, new ClientDisconnectedArgs(processId, cause: null));

        }
        else
        {
            OnMessageReceived?.Invoke(this, new MessageFromClientEventArgs(message, processId));

        }
        #pragma warning restore GU0011
    }
}