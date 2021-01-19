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
    public sealed class SharmComunicator : IDisposable
    {
        public static readonly string ProcessId = Guid.NewGuid().ToString("N");

        private ISharmIpc _sharmIpc = new Dummy();
        private readonly string _globalId;
        private readonly bool _master;
        private readonly NetworkMessageFormatter _formatter = NetworkMessageFormatter.Shared;

        public event Action<NetworkMessage>? OnMessage;

        public SharmComunicator(string globalId, bool master)
        {
            _globalId = globalId;
            _master = master;
        }

        public void Dispose() => _sharmIpc.Dispose();

        public void Connect()
        {
            _sharmIpc = new Connection(_globalId);
            _sharmIpc.OnMessage += OnOnMessage;
        }

        public void Send(NetworkMessage msg, string target)
        {
            var toSend = _formatter.WriteMessage(msg, i =>
                                                      {
                                                          var memory = MemoryPool<byte>.Shared.Rent(i + 32);

                                                          Encoding.ASCII.GetBytes(ProcessId, memory.Memory.Span);

                                                          return (memory, 31);
                                                      });

            using var data = toSend.Message;
            _sharmIpc.Send(data.Memory[..toSend.Lenght].ToArray());
        }

        private void OnOnMessage(NetworkMessage obj) => OnMessage?.Invoke(obj);

        private interface ISharmIpc : IDisposable
        {
            event Action<NetworkMessage> OnMessage;

            void Send(byte[] msg);
        }

        private sealed class Dummy : ISharmIpc
        {
            public void Dispose()
            {
                
            }

            public event Action<NetworkMessage> OnMessage
            {
                add {  }
                remove { }
            }

            public void Send(byte[] msg)
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
                if (id.StartsWith("All") || id == ProcessId) OnMessage?.Invoke(_messageFormatter.ReadMessage(arg2.AsMemory()[31..]));
            }

            public void Dispose() => _sharmIpc.Dispose();

            public event Action<NetworkMessage>? OnMessage;

            public void Send(byte[] msg)
            {
                if (_sharmIpc.RemoteRequestWithoutResponse(msg))
                    throw new InvalidOperationException("Message was not send");
            }
        }
    }

    public sealed class SharmServer : IDataServer
    {
        private readonly Semaphore _lock;
        private readonly SharmComunicator _comunicator;

        public SharmServer(string uniqeName)
        {


            if(Semaphore.TryOpenExisting($"Global\\{uniqeName}-Semaphore"))
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
        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Send(string client, NetworkMessage message)
        {
            throw new NotImplementedException();
        }
    }
}