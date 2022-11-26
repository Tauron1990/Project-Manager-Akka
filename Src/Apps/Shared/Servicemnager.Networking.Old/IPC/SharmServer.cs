using System;
using Servicemnager.Networking.Data;
using Servicemnager.Networking.Server;
using SuperSimpleTcp;

namespace Servicemnager.Networking.IPC;

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

    public void Start() => _comunicator.Connect();

    public bool Send(in Client client, NetworkMessage message) => _comunicator.Send(message, client);

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
            ClientDisconnected?.Invoke(this, new ClientDisconnectedArgs(processId, DisconnectReason.Normal));

        }
        else
        {
            OnMessageReceived?.Invoke(this, new MessageFromClientEventArgs(message, processId));

        }
        #pragma warning restore GU0011
    }
}