using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Servicemnager.Networking.Data;
using SuperSimpleTcp;

namespace Servicemnager.Networking.Server;

public sealed class DataServer : IDataServer
{
    private readonly ConcurrentDictionary<string, MessageBuffer> _clients = new(StringComparer.Ordinal);
    private readonly NetworkMessageFormatter _messageFormatter = NetworkMessageFormatter.Shared;
    private readonly SimpleTcpServer _server;

    private EndPoint? _endPoint;

    public DataServer(string host, int port = 0)
    {
        _server = new SimpleTcpServer(host, port, ssl: false, pfxCertFilename: null, pfxPassword: null)
                  {
                      Keepalive = { EnableTcpKeepAlives = true },
                  };

        _server.Events.ClientConnected += (_, args) =>
                                          {
                                              _clients.TryAdd(args.IpPort, new MessageBuffer(MemoryPool<byte>.Shared));
                                              ClientConnected?.Invoke(this, new ClientConnectedArgs(Client.From(args.IpPort)));
                                          };
        _server.Events.ClientDisconnected += (sender, args) =>
                                             {
                                                 _clients.TryRemove(args.IpPort, out _);
                                                 ClientDisconnected?.Invoke(this, new ClientDisconnectedArgs(Client.From(args.IpPort), args.Reason));
                                             };
        _server.Events.DataReceived += EventsOnDataReceived;
    }

    public EndPoint EndPoint
        // ReSharper disable once ConstantNullCoalescingCondition
        => _endPoint ??=
            ((TcpListener)_server.GetType().GetField("_listener", BindingFlags.Instance | BindingFlags.NonPublic)
              ?.GetValue(_server)!).LocalEndpoint;

    public event EventHandler<ClientConnectedArgs>? ClientConnected;
    public event EventHandler<ClientDisconnectedArgs>? ClientDisconnected;
    public event EventHandler<MessageFromClientEventArgs>? OnMessageReceived;

    public void Start() => _server.Start();

    public bool Send(in Client client, NetworkMessage message)
    {
        var data = _messageFormatter.WriteMessage(message);
        using var memory = data.Message;

        _server.Send(client.Value, memory.Memory[..data.Lenght].ToArray());

        return true;
    }

    public void Dispose() => _server.Dispose();

    private void EventsOnDataReceived(object? sender, DataReceivedEventArgs e)
    {
        MessageBuffer buffer = _clients.GetOrAdd(e.IpPort, _ => new MessageBuffer(MemoryPool<byte>.Shared));
        NetworkMessage? msg = buffer.AddBuffer(e.Data);

        if(msg != null)
            OnMessageReceived?.Invoke(this, new MessageFromClientEventArgs(msg, Client.From(e.IpPort)));
    }
}