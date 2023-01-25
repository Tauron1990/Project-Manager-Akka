using System.Net.Sockets;
using System.Threading.Channels;
using JetBrains.Annotations;
using Tauron.Servicemnager.Networking.Data;

namespace Tauron.Servicemnager.Networking.Server;

[PublicAPI]
public sealed class DataClient : IDataClient
{
    private readonly TcpClient _client;
    private readonly MessageReader<NetworkMessage> _messageReader;// = new(NetworkMessageFormatter.Shared,  MemoryPool<byte>.Shared);
    private readonly NetworkMessageFormatter _messageFormatter = NetworkMessageFormatter.Shared;
    private Channel<NetworkMessage> _onMessageReceived;

    public DataClient(string host, int port = 0)
    {
        _client = new TcpClient(host, port);
        _messageReader = new MessageReader<NetworkMessage>(new SocketMessageStream(_client.Client), _messageFormatter);
        _onMessageReceived = Channel.CreateUnbounded<NetworkMessage>();
    }

    public Task Run(CancellationToken token)
        => _messageReader.ReadAsync(_onMessageReceived.Writer, token);

    public void Close()
        => _client.Close();

    public ChannelReader<NetworkMessage> OnMessageReceived => _onMessageReceived.Reader;

    public bool Send(NetworkMessage msg)
    {
        var data = _messageFormatter.WriteMessage(msg);
        using var memory = data.Message;

        _client.GetStream().Write(memory.Memory.Span);

        return true;
    }

    public void Dispose()
        => _client.Dispose();
}