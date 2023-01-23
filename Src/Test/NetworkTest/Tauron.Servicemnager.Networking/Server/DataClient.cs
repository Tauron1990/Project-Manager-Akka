using System.Buffers;
using JetBrains.Annotations;
using SuperSimpleTcp;
using Tauron.Servicemnager.Networking.Data;

namespace Tauron.Servicemnager.Networking.Server;

[PublicAPI]
public sealed class DataClient : IDataClient
{
    private readonly SimpleTcpClient _client;
    private readonly MessageReader<NetworkMessage> _messageReader = new(NetworkMessageFormatter.Shared,  MemoryPool<byte>.Shared);
    private readonly NetworkMessageFormatter _messageFormatter = NetworkMessageFormatter.Shared;

    public DataClient(string host, int port = 0)
    {
        _client = new SimpleTcpClient(host, port, ssl: false, pfxCertFilename: null, pfxPassword: null)
                  {
                      Keepalive = { EnableTcpKeepAlives = true },
                  };

        _client.Events.Connected += (_, args) => Connected?.Invoke(this, new ClientConnectedArgs(Client.From(args.IpPort)));
        _client.Events.Disconnected += (_, args)
                                           => Disconnected?.Invoke(this, new ClientDisconnectedArgs(Client.From(args.IpPort), args.Reason));

        _client.Events.DataReceived += (_, args) =>
                                       {
                                           NetworkMessage? msg = _messageReader.AddBuffer(args.Data);
                                           if(msg != null)
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