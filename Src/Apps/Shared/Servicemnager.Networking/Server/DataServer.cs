using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Servicemnager.Networking.Data;
using SimpleTcp;

namespace Servicemnager.Networking.Server
{
    public sealed class MessageFromClientEventArgs : EventArgs
    {
        public NetworkMessage Message { get; }

        public string Client { get; }

        public MessageFromClientEventArgs(NetworkMessage message, string client)
        {
            Message = message;
            Client = client;
        }
    }

    public sealed class DataServer : IDisposable
    {
        private readonly SimpleTcpServer _server;

        private readonly ConcurrentDictionary<string, MessageBuffer> _clients = new();
        private readonly NetworkMessageFormatter _messageFormatter = new(MemoryPool<byte>.Shared);

        private EndPoint? _endPoint;

        public EndPoint EndPoint 
            // ReSharper disable once ConstantNullCoalescingCondition
            => (_endPoint ??= ((TcpListener) _server.GetType().GetField("_Listener", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(_server)!).LocalEndpoint!)!;

        public DataServer(string host, int port = 0)
        {
            _server = new SimpleTcpServer(host, port, false, null, null)
                      {
                          Keepalive = {EnableTcpKeepAlives = true}
                      };

            _server.Events.ClientConnected += (_, args) => _clients.TryAdd(args.IpPort, new MessageBuffer(MemoryPool<byte>.Shared));
            _server.Events.ClientDisconnected += (sender, args) => _clients.TryRemove(args.IpPort, out _);
            _server.Events.DataReceived += EventsOnDataReceived;
        }

        private void EventsOnDataReceived(object? sender, DataReceivedEventArgs e)
        {
            var buffer = _clients.GetOrAdd(e.IpPort, _ => new MessageBuffer(MemoryPool<byte>.Shared));
            var msg = buffer.AddBuffer(e.Data);

            if(msg != null)
                OnMessageReceived?.Invoke(this, new MessageFromClientEventArgs(msg, e.IpPort));
        }

        public event EventHandler<ClientConnectedEventArgs> ClientConnected
        {
            add => _server.Events.ClientConnected += value;
            remove => _server.Events.ClientConnected -= value;
        }

        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected
        {
            add => _server.Events.ClientDisconnected += value;
            remove => _server.Events.ClientDisconnected -= value;
        }

        public event EventHandler<MessageFromClientEventArgs>? OnMessageReceived;

        public void Start() => _server.Start();

        public void Send(string client, NetworkMessage message)
        {
            var data = _messageFormatter.WriteMessage(message);
            using var memory = data.Message;

            _server.Send(client, memory.Memory[..data.Lenght].ToArray());
        }

        public void Dispose() => _server.Dispose();
    }
}