using System;
using System.Buffers;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using NLog;
using Servicemnager.Networking;
using Servicemnager.Networking.Data;
using Servicemnager.Networking.IPC;
using Tauron;

namespace WpfApp
{
    internal sealed class IpcConnection : IDisposable
    {
        private readonly Subject<NetworkMessage> _messageHandler = new();
        private IDataClient? _dataClient;
        private IDataServer? _dataServer;

        public IpcConnection(bool masterExists, IpcApplicationType type, Action<string, Exception> errorHandler)
        {
            try
            {
                switch (type)
                {
                    case IpcApplicationType.Server:
                        if (masterExists)
                        {
                            IsReady = false;
                            ErrorMessage = "Duplicate Server Start";
                            return;
                        }

                        _dataServer = new SharmServer(MainWindowModel.Id, errorHandler);
                        _dataServer.OnMessageReceived += (_, args) => _messageHandler.OnNext(args.Message);
                        break;
                    case IpcApplicationType.Client:
                        if (!masterExists)
                        {
                            IsReady = false;
                            ErrorMessage = "No Server Found";
                            return;
                        }

                        _dataClient = new SharmClient(MainWindowModel.Id, errorHandler);
                        _dataClient.OnMessageReceived += (_, args) => _messageHandler.OnNext(args.Message);
                        break;
                    case IpcApplicationType.NoIpc:
                        IsReady = false;
                        ErrorMessage = "Ipc Disabled";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                IsReady = false;
            }
        }

        public string ErrorMessage { get; private set; } = string.Empty;

        public bool IsReady { get; private set; } = true;

        public void Dispose()
        {
            _messageHandler.Dispose();
            (_dataClient as IDisposable)?.Dispose();
            _dataServer?.Dispose();

            _dataClient = null;
            _dataServer = null;
        }

        public IObservable<CallResult<TType>> OnMessage<TType>()
        {
            if (!IsReady)
                return Observable.Empty<CallResult<TType>>();

            string type = typeof(TType).AssemblyQualifiedName ??
                          throw new InvalidOperationException("Invalid Message Type");

            return _messageHandler.Where(nm => nm.Type == type)
                .SelectSafe(nm => JsonConvert.DeserializeObject<TType>(Encoding.UTF8.GetString(nm.Data))).Isonlate();
        }

        public bool SendMessage<TMessage>(string to, TMessage message)
        {
            if (!IsReady)
                return false;

            var name = typeof(TMessage).AssemblyQualifiedName ??
                       throw new InvalidOperationException("Invalid Message Type");
            var data = JsonConvert.SerializeObject(message);

            var nm = NetworkMessage.Create(name, Encoding.UTF8.GetBytes(data));
            if (_dataClient != null)
                return _dataClient.Send(nm);
            return _dataServer != null && _dataServer.Send(to, nm);
        }

        public bool SendMessage<TMessage>(TMessage message) => SendMessage("All", message);

        public void Start()
        {
            try
            {
                _dataServer?.Start();

                if (_dataClient == null) return;

                _dataClient.Connect();
            }
            catch (Exception e)
            {
                IsReady = false;
                ErrorMessage = e.Message;

                LogManager.GetCurrentClassLogger()
                    .Error(e, "Error on Starting Ipc");

                Dispose();
            }
        }

        public void Disconnect()
        {
            if (_dataClient is SharmClient client)
                client.Disconnect();
        }
    }

    #if DEBUG

    internal sealed class DebugConnection : SharmComunicator.ISharmIpc
    {
        private static readonly NetworkMessageFormatter MessageFormatter = new(MemoryPool<byte>.Shared);
        private static DebugConnection? _master;
        private static readonly List<DebugConnection> _clients = new();

        private readonly Mutex? _mutex;
        private ulong _msgId = 1;

        public DebugConnection(string id)
        {
            if (_master == null)
            {
                _mutex = new Mutex(true, id + "SharmNet_MasterMutex");
                _mutex.WaitOne();
                _master = this;
            }
            else
            {
                _clients.Add(this);
            }
        }

        public void Dispose()
        {
            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex.Dispose();

                _master = null;
            }
            else
            {
                _clients.Remove(this);
            }
        }

        public event SharmMessageHandler? OnMessage;

        public bool Send(byte[] arg2)
        {
            string id = Encoding.ASCII.GetString(arg2, 0, 32).Trim();
            string from = Encoding.ASCII.GetString(arg2, 31, 32).Trim();
            string processid = SharmComunicator.ProcessId;
            var test = processid == from;

            var msg = MessageFormatter.ReadMessage(arg2.AsMemory()[63..]);

            if (_mutex == null)
                _master?.OnMessage?.Invoke(msg, _msgId, from);
            else
                foreach (var client in _clients)
                    client.OnMessage?.Invoke(msg, _msgId, @from);

            _msgId++;
            return true;
        }
    }

    #endif
}