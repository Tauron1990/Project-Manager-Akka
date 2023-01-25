using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using JetBrains.Annotations;
using Tauron.Servicemnager.Networking.Data;

namespace Tauron.Servicemnager.Networking.Server;

[PublicAPI]
public sealed class DataServer : IDataServer
{
    private sealed record ClientMeta(Socket Socket, MessageReader<NetworkMessage> Reader, CancellationTokenSource Token, Task Runner)
        : IDisposable
    {
        public void Dispose()
        {
            Socket.Close();
            Token.Cancel();
            Reader.Dispose();
            Socket.Dispose();
            Token.Dispose();
        }
    }

    private readonly ConcurrentDictionary<string, ClientMeta> _clients = new(StringComparer.Ordinal);
    private readonly TcpListener _tcpListener;
    private readonly CancellationTokenSource _cancellation = new();
    private readonly NetworkMessageFormatter _messageFormatter = NetworkMessageFormatter.Shared;
    
    public DataServer(string host, int port = 0)
    {
        _tcpListener = new TcpListener(new IPEndPoint(IPAddress.Parse(host), port));
        _tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, optionValue: true);
    }

    public void Dispose()
    {
        _cancellation.Cancel();
        
        foreach (ClientMeta clientsValue in _clients.Values)
        {
            clientsValue.Dispose();
        }
        
        _tcpListener.Stop();
        _cancellation.Dispose();
    }

    public event EventHandler<ClientConnectedArgs>? ClientConnected;
    public event EventHandler<ClientDisconnectedArgs>? ClientDisconnected;
    public event EventHandler<MessageFromClientEventArgs>? OnMessageReceived;

    public async Task Start()
    {
        try
        {
            await AcceptClients().ConfigureAwait(false);
        }
        catch(OperationCanceledException){}
    }

    private async Task AcceptClients()
    {
        _tcpListener.Start();
        
        while (!_cancellation.IsCancellationRequested)
        {
            if(_tcpListener.Pending())
            {
                Socket client = await _tcpListener.AcceptSocketAsync(_cancellation.Token).ConfigureAwait(false);
                client.SetKeepAlive(1000, 1000);
                var name = client.RemoteEndPoint!.ToString();
                Debug.Assert(!string.IsNullOrWhiteSpace(name), "Client Ip is Null or Empty");
                
                var reader = new MessageReader<NetworkMessage>(new SocketMessageStream(client), _messageFormatter);
                var cancel = CancellationTokenSource.CreateLinkedTokenSource(_cancellation.Token);
                
                var meta = new ClientMeta(
                    client,
                    reader,
                    cancel, 
                    ClientRunner(name, reader, cancel.Token));

                ClientConnected?.Invoke(this, new ClientConnectedArgs(Client.From(name)));
                
                _clients[name] = meta;
            }
            else
                await Task.Delay(1000, _cancellation.Token).ConfigureAwait(false);
        }
    }

    private async Task ClientRunner(string name, MessageReader<NetworkMessage> reader, CancellationToken token)
    {
        try
        {
            var pair = Channel.CreateUnbounded<NetworkMessage>();

            Task readTask = reader.ReadAsync(pair.Writer, token);
            Task drainTask = DrainChannel(pair.Reader, name, token);

            await Task.WhenAll(readTask, drainTask).ConfigureAwait(false);
            
            ClientDisconnected?.Invoke(this, new ClientDisconnectedArgs(Client.From(name), cause: null));
        }
        catch(OperationCanceledException){}
        catch (Exception e)
        {
            ClientDisconnected?.Invoke(this, new ClientDisconnectedArgs(Client.From(name), e));
        }
    }

    private async Task DrainChannel(ChannelReader<NetworkMessage> channelReader, string name, CancellationToken token)
    {
        await foreach (NetworkMessage msg in channelReader.ReadAllAsync(token).ConfigureAwait(false))
            OnMessageReceived?.Invoke(this, new MessageFromClientEventArgs(msg, Client.From(name)));
    }

    public async ValueTask<bool> Send(Client client, NetworkMessage message)
    {
        if(!_clients.TryGetValue(client.Value, out ClientMeta? clientData))
        {
            if(client != Client.All)
                return false;
        }

        var msg = _messageFormatter.WriteMessage(message);
        using var data = msg.Message;
        var actualData = data.Memory[..msg.Lenght];
        
        if(client == Client.All)
        {
            foreach (var meta in _clients)
                await meta.Value.Socket.SendAsync(actualData, _cancellation.Token).ConfigureAwait(false);

            return true;
        }
        
        await clientData!.Socket.SendAsync(actualData, _cancellation.Token).ConfigureAwait(false);

        return true;

    }
}