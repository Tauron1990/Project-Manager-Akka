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
        #if DEBUG
        public static Func<ISharmIpc>? ConnectionFac { get; set; }
        #endif

        public static readonly string ProcessId = Guid.NewGuid().ToString("N");

        public static bool MasterIpcReady(string id)
        {
            try
            {
                using var mt = new Mutex(true, id + "SharmNet_MasterMutex", out var created);
                if (!created) return true;

                mt.ReleaseMutex();
                return false;

            }
            catch (AbandonedMutexException)
            {
                return false;
            }
        }

        private ISharmIpc _sharmIpc = new Dummy();
        private readonly string _globalId;
        private readonly Action<string, Exception> _errorHandler;
        private readonly NetworkMessageFormatter _formatter = NetworkMessageFormatter.Shared;

        public event SharmMessageHandler? OnMessage;

        public SharmComunicator(string globalId, Action<string, Exception> errorHandler)
        {
            _globalId = globalId;
            _errorHandler = errorHandler;
        }

        public void Dispose() => _sharmIpc.Dispose();

        public void Connect()
        {
#if DEBUG

            _sharmIpc = ConnectionFac == null ? new Connection(_globalId, _errorHandler) : ConnectionFac();

#else
            _sharmIpc = new Connection(_globalId, _errorHandler);  
            #endif
            _sharmIpc.OnMessage += (message, messageId, processsId) => OnMessage?.Invoke(message, messageId, processsId);
        }

        public bool Send(NetworkMessage msg, string target)
        {
            target = target.Length switch
                     {
                         > 32 => throw new ArgumentOutOfRangeException(nameof(target), target, "Id Longer then 32 Chars"),
                         < 32 => target.PadRight(32),
                         _    => target
                     };

            var toSend = _formatter.WriteMessage(msg, i =>
                                                      {
                                                          var memory = MemoryPool<byte>.Shared.Rent(i + 64);

                                                          Encoding.ASCII.GetBytes(target, memory.Memory.Span);
                                                          Encoding.ASCII.GetBytes(ProcessId, memory.Memory.Span[31..]);

                                                          return (memory, 63);
                                                      });

            using var data = toSend.Message;
            return _sharmIpc.Send(data.Memory[..toSend.Lenght].ToArray());
        }

#if DEBUG
        public 
#else
        private
#endif
        interface ISharmIpc : IDisposable
        {
            event SharmMessageHandler OnMessage;

            bool Send(byte[] msg);
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

            public bool Send(byte[] msg) => throw new InvalidOperationException("Ipc Not Started");
        }

        private sealed class Connection : ISharmIpc
        {
            private readonly SharmIpc _sharmIpc;
            private readonly NetworkMessageFormatter _messageFormatter = NetworkMessageFormatter.Shared;

            public Connection(string globalId, Action<string, Exception> errorHandler) 
                => _sharmIpc = new SharmIpc(globalId, Handle, ExternalExceptionHandler:errorHandler, protocolVersion: SharmIpc.eProtocolVersion.V2);

            private void Handle(ulong arg1, byte[] arg2)
            {
                string id = Encoding.ASCII.GetString(arg2, 0, 32).Trim();
                string from = Encoding.ASCII.GetString(arg2, 31, 32).Trim();
                if (id.StartsWith("All") || id == ProcessId) OnMessage?.Invoke(_messageFormatter.ReadMessage(arg2.AsMemory()[63..]), arg1, from);
            }

            public void Dispose() => _sharmIpc.Dispose();

            public event SharmMessageHandler? OnMessage;

            public bool Send(byte[] msg) => _sharmIpc.RemoteRequestWithoutResponse(msg);
        }
    }

    public sealed class SharmServer : IDataServer
    {
        private readonly SharmComunicator _comunicator;

        public SharmServer(string uniqeName, Action<string, Exception> errorHandler)
        {
            _comunicator = new SharmComunicator(uniqeName, errorHandler);
            _comunicator.OnMessage += ComunicatorOnOnMessage;
        }

        private void ComunicatorOnOnMessage(NetworkMessage message, ulong messageId, string processId)
        {
            switch (message.Type)
            {
                case SharmComunicatorMessages.RegisterClient:
                    _comunicator.Send(message, processId);
                    ClientConnected?.Invoke(this, new ClientConnectedArgs(processId));
                    break;
                case SharmComunicatorMessages.UnRegisterClient:
                    _comunicator.Send(message, processId);
                    ClientDisconnected?.Invoke(this, new ClientDisconnectedArgs(processId, DisconnectReason.Normal));
                    break;
                default:
                    OnMessageReceived?.Invoke(this, new MessageFromClientEventArgs(message, processId));
                    break;
            }
        }

        public void Dispose() => _comunicator.Dispose();

        public event EventHandler<ClientConnectedArgs>? ClientConnected;

        public event EventHandler<ClientDisconnectedArgs>? ClientDisconnected;

        public event EventHandler<MessageFromClientEventArgs>? OnMessageReceived;

        public void Start() => _comunicator.Connect();

        public bool Send(string client, NetworkMessage message) => _comunicator.Send(message, client);
    }

    public sealed class SharmClient : IDataClient, IDisposable
    {
        private readonly SharmComunicator _comunicator;

        public SharmClient(string uniqeName, Action<string, Exception> errorHandler)
        {
            _comunicator = new SharmComunicator(uniqeName, errorHandler);
            _comunicator.OnMessage += ComunicatorOnOnMessage;
        }

        private void ComunicatorOnOnMessage(NetworkMessage message, ulong messageid, string processsid)
        {
            switch (message.Type)
            {
                case SharmComunicatorMessages.RegisterClient:
                    Connected?.Invoke(this, new ClientConnectedArgs(processsid));
                    break;
                case SharmComunicatorMessages.UnRegisterClient:
                    Disconnected?.Invoke(this, new ClientDisconnectedArgs(processsid, DisconnectReason.Normal));
                    Dispose();
                    break;
                default:
                    OnMessageReceived?.Invoke(this, new MessageFromServerEventArgs(message));
                    break;
            }
        }

        public void Connect()
        {
            _comunicator.Connect();
            Send(NetworkMessage.Create(SharmComunicatorMessages.RegisterClient));
        }

        public event EventHandler<ClientConnectedArgs>? Connected;
        public event EventHandler<ClientDisconnectedArgs>? Disconnected;
        public event EventHandler<MessageFromServerEventArgs>? OnMessageReceived;

        public bool Send(NetworkMessage msg) 
            => _comunicator.Send(msg, "All");

        public void Disconnect() 
            => _comunicator.Send(NetworkMessage.Create(SharmComunicatorMessages.UnRegisterClient), "All");

        public void Dispose() => _comunicator.Dispose();
    }
}