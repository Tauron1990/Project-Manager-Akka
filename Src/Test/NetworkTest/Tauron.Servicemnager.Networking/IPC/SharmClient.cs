using System.Threading.Channels;
using Tauron.Servicemnager.Networking.Data;

namespace Tauron.Servicemnager.Networking.IPC;

public sealed class SharmClient : IDataClient
{
    private readonly SharmComunicator _comunicator;
    private readonly Channel<NetworkMessage> _channel;

    public SharmClient(SharmProcessId uniqeName, Action<string, Exception> errorHandler)
    {
        _comunicator = new SharmComunicator(uniqeName, errorHandler);
        _comunicator.OnMessage += ComunicatorOnOnMessage;
        _channel = Channel.CreateUnbounded<NetworkMessage>();
    }

    public Task Run(CancellationToken token)
    {
        _comunicator.Connect();

        bool result = Send(NetworkMessage.Create(SharmComunicatorMessage.RegisterClient.Value));
        if(result)
            return Task.CompletedTask;

        return Task.FromException(new InvalidOperationException("First Message Send Failed"));
    }

    public ChannelReader<NetworkMessage> OnMessageReceived => _channel.Reader;

    public bool Send(NetworkMessage msg)
        => _comunicator.Send(msg, Client.All);

    public void Dispose()
    {
        Close();
        _comunicator.Dispose();
    }

    private void ComunicatorOnOnMessage(NetworkMessage message, ulong messageid, in Client processsid)
    {
        if(message.Type == SharmComunicatorMessage.RegisterClient)
            return;

        if(message.Type == SharmComunicatorMessage.UnRegisterClient)
        {
            Dispose();
            return;
        }
#pragma warning disable MA0134
        _channel.Writer.WriteAsync(message);
    }

    public void Close()
    {
        _comunicator.Send(NetworkMessage.Create(SharmComunicatorMessage.UnRegisterClient.Value), Client.All);
        _channel.Writer.Complete();
    }
}