using System;
using System.Buffers;
using Servicemnager.Networking.Data;
using SimpleTcp;

namespace Servicemnager.Networking.Server
{
    public class MessageFromServerEventArgs : EventArgs
    {
        public NetworkMessage Message { get; }

        public MessageFromServerEventArgs(NetworkMessage message) => Message = message;
    }

    public sealed class DataClient : IDataClient
    {
        private readonly SimpleTcpClient _client;
        private readonly MessageBuffer _messageBuffer = new(MemoryPool<byte>.Shared);
        private readonly NetworkMessageFormatter _messageFormatter = NetworkMessageFormatter.Shared;

        public DataClient(string host, int port = 0)
        {
            _client = new SimpleTcpClient(host, port, false, null, null)
                      {
                          Keepalive = {EnableTcpKeepAlives = true}
                      };

            _client.Events.DataReceived += (_, args) =>
                                           {
                                               var msg = _messageBuffer.AddBuffer(args.Data);
                                               if(msg != null)
                                                   OnMessageReceived?.Invoke(this, new MessageFromServerEventArgs(msg));
                                           };
        }

        public void Connect() => _client.Connect();

        public event EventHandler<ClientConnectedEventArgs>? Connected
        {
            add => _client.Events.Connected += value;
            remove => _client.Events.Connected -= value;
        }

        public event EventHandler<ClientDisconnectedEventArgs>? Disconnected
        {
            add => _client.Events.Disconnected += value;
            remove => _client.Events.Disconnected -= value;
        }

        public event EventHandler<MessageFromServerEventArgs>? OnMessageReceived;

        public void Send(NetworkMessage msg)
        {
            var data = _messageFormatter.WriteMessage(msg);
            using var memory = data.Message;

            _client.Send(memory.Memory[..data.Lenght].ToArray());
        }
    }
}