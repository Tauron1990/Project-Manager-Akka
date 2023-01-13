using SuperSimpleTcp;
using Tauron.Servicemnager.Networking.Data;

namespace Tauron.Servicemnager.Networking.IPC;

public sealed class SharmClient : IDataClient, IDisposable
{
    private readonly SharmComunicator _comunicator;

    public SharmClient(SharmProcessId uniqeName, Action<string, Exception> errorHandler)
    {
        _comunicator = new SharmComunicator(uniqeName, errorHandler);
        _comunicator.OnMessage += ComunicatorOnOnMessage;
    }

    public bool Connect()
    {
        _comunicator.Connect();

        return Send(NetworkMessage.Create(SharmComunicatorMessage.RegisterClient.Value));
    }

    public event EventHandler<ClientConnectedArgs>? Connected;
    public event EventHandler<ClientDisconnectedArgs>? Disconnected;
    public event EventHandler<MessageFromServerEventArgs>? OnMessageReceived;

    public bool Send(NetworkMessage msg)
        => _comunicator.Send(msg, Client.All);

    public void Dispose() => _comunicator.Dispose();

    private void ComunicatorOnOnMessage(NetworkMessage message, ulong messageid, in Client processsid)
    {
        if(message.Type == SharmComunicatorMessage.RegisterClient)
        {
            Connected?.Invoke(this, new ClientConnectedArgs(processsid));
        }
        else if(message.Type == SharmComunicatorMessage.UnRegisterClient)
        {
            Disconnected?.Invoke(this, new ClientDisconnectedArgs(processsid, DisconnectReason.Normal));
            Dispose();
        }
        else
        {
            OnMessageReceived?.Invoke(this, new MessageFromServerEventArgs(message));
        }
    }

    public void Disconnect()
        => _comunicator.Send(NetworkMessage.Create(SharmComunicatorMessage.UnRegisterClient.Value), Client.All);
}