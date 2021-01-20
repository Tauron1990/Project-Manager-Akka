using System;
using System.Buffers;
using System.Text;
using System.Threading;
using Servicemnager.Networking.Data;
using Servicemnager.Networking.Server;
using SimpleTcp;
using tiesky.com;

namespace Servicemnager.Networking.IPC
{
    public delegate void SharmMessageHandler(NetworkMessage message, ulong messageId, string processsId);

    public sealed class SharmComunicator : IDisposable
    {
        public static readonly string ProcessId = Guid.NewGuid().ToString("N");

        private ISharmIpc _sharmIpc = new Dummy();
        private readonly string _globalId;
        private readonly NetworkMessageFormatter _formatter = NetworkMessageFormatter.Shared;

        public event SharmMessageHandler? OnMessage;

        public SharmComunicator(string globalId) => _globalId = globalId;

        public void Dispose() => _sharmIpc.Dispose();

        public void Connect()
        {
            _sharmIpc = new Connection(_globalId);
            _sharmIpc.OnMessage += (message, messageId, processsId) => OnMessage?.Invoke(message, messageId, processsId);
        }

        public void Send(NetworkMessage msg, string target, ulong? msgId = null)
        {
            target = target.Length switch
                     {
                         > 32 => throw new ArgumentOutOfRangeException(nameof(target), target, "Id Longer then 32 Chars"),
                         < 32 => target.PadRight(32),
                         _    => target
                     };

            var toSend = _formatter.WriteMessage(msg, i =>
                                                      {
                                                          var memory = MemoryPool<byte>.Shared.Rent(i + 32);

                                                          Encoding.ASCII.GetBytes(target, memory.Memory.Span);

                                                          return (memory, 31);
                                                      });

            using var data = toSend.Message;
            _sharmIpc.Send(data.Memory[..toSend.Lenght].ToArray(), msgId);
        }
        
        private interface ISharmIpc : IDisposable
        {
            event SharmMessageHandler OnMessage;

            void Send(byte[] msg, ulong? msgId = null);
        }

        private sealed class Dummy : ISharmIpc
        {
            public void Dispose()
            {
                
            }

            public event SharmMessageHandler OnMessage
            {
                add {  }
                remove { }
            }

            public void Send(byte[] msg, ulong? msgId = null)
            {
                throw new InvalidOperationException("Ipc Not Started");
            }
        }

        private sealed class Connection : ISharmIpc
        {
            private readonly SharmIpc _sharmIpc;
            private readonly NetworkMessageFormatter _messageFormatter = NetworkMessageFormatter.Shared;

            public Connection(string globalId)
            {
                globalId = "Global\\" + globalId;
                _sharmIpc = new SharmIpc(globalId, Handle, protocolVersion: SharmIpc.eProtocolVersion.V2);
            }

            private void Handle(ulong arg1, byte[] arg2)
            {
                string id = Encoding.ASCII.GetString(arg2, 0, 32);
                if (id.StartsWith("All") || id == ProcessId) OnMessage?.Invoke(_messageFormatter.ReadMessage(arg2.AsMemory()[31..]), arg1, id);
            }

            public void Dispose() => _sharmIpc.Dispose();

            public event SharmMessageHandler? OnMessage;

            public void Send(byte[] msg, ulong? msgId = null)
            {
                switch (msgId)
                {
                    case null when _sharmIpc.RemoteRequestWithoutResponse(msg):
                        return;
                    case null:
                        throw new InvalidOperationException("Message was not send");
                    default:
                        _sharmIpc.AsyncAnswerOnRemoteCall(msgId.Value, Tuple.Create(true, msg));
                        return;
                }
            }
        }
    }

    public sealed class SharmServer : IDataServer
    {
        internal static string MakeServerName(string uniqeName)
            => $"Global\\{uniqeName}-SharmServer-Semaphore";

        private readonly Semaphore _lock;
        private readonly SharmComunicator _comunicator;

        public SharmServer(string uniqeName)
        {
            _lock = new Semaphore(0, 1, MakeServerName(uniqeName));
            _comunicator = new SharmComunicator(uniqeName);

            _comunicator.OnMessage += OnComunicatorOnOnMessage;
        }

        private void OnComunicatorOnOnMessage(NetworkMessage message, ulong messageId)
        {
            switch (message.Type)
            {
                case SharmComunicatorMessages.RegisterClient:
                    break;
                default:
                    OnMessageReceived?.Invoke(this, new MessageFromClientEventArgs(message, messageId.ToString()));
                    break;
            }
        }

        public void Dispose()
        {
            _lock.Release();
            _lock.Dispose();
            _comunicator.Dispose();
        }

        public event EventHandler<ClientConnectedEventArgs> ClientConnected;

        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;

        public event EventHandler<MessageFromClientEventArgs>? OnMessageReceived;

        public void Start() => _comunicator.Connect();

        public void Send(string client, NetworkMessage message) => _comunicator.Send(message, client);
    }
}