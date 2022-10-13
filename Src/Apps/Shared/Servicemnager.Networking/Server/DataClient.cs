using System;
using System.Buffers;
using Servicemnager.Networking.Data;
using SuperSimpleTcp;

namespace Servicemnager.Networking.Server;

public class MessageFromServerEventArgs : EventArgs
{
    public MessageFromServerEventArgs(NetworkMessage message) => Message = message;
    public NetworkMessage Message { get; }
}

public sealed class DataClient : IDataClient
{
    private readonly SimpleTcpClient _client;
    private readonly MessageBuffer _messageBuffer = new(MemoryPool<byte>.Shared);
    private readonly NetworkMessageFormatter _messageFormatter = NetworkMessageFormatter.Shared;

    public DataClient(string host, int port = 0)
    {
        _client = new SimpleTcpClient(host, port, ssl: false, pfxCertFilename: null, pfxPassword: null)
                  {
                      Keepalive = { EnableTcpKeepAlives = true }
                  };

        _client.Events.Connected += (_, args) => Connected?.Invoke(this, new ClientConnectedArgs(Client.From(args.IpPort)));
        _client.Events.Disconnected += (_, args)
                                           => Disconnected?.Invoke(this, new ClientDisconnectedArgs(Client.From(args.IpPort), args.Reason));

        _client.Events.DataReceived += (_, args) =>
                                       {
                                           var msg = _messageBuffer.AddBuffer(args.Data);
                                           if (msg != null)
                                               OnMessageReceived?.Invoke(this, new MessageFromServerEventArgs(msg));
                                       };
    }

    public bool Connect()
    {
        _client.Connect();

        return true;
    }

    public event EventHandler<ClientConnectedArgs>? Connected;

    public event EventHandler<ClientDisconnectedArgs>? Disconnected;

    public event EventHandler<MessageFromServerEventArgs>? OnMessageReceived;

    public bool Send(NetworkMessage msg)
    {
        var data = _messageFormatter.WriteMessage(msg);
        using var memory = data.Message;

        _client.Send(memory.Memory[..data.Lenght].ToArray());

        return true;
    }
}