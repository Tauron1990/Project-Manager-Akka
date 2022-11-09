using System;
using System.Buffers;
using System.Text;
using System.Threading;
using Servicemnager.Networking.Data;
using Servicemnager.Networking.Server;
using SuperSimpleTcp;
using tiesky.com;

namespace Servicemnager.Networking.IPC;

public delegate void SharmMessageHandler(NetworkMessage message, ulong messageId, in Client processsId);

public sealed class SharmComunicator : IDisposable
{
    public static readonly SharmProcessId ProcessId = SharmProcessId.From(Guid.NewGuid().ToString("N"));
    private readonly Action<string, Exception> _errorHandler;
    private readonly NetworkMessageFormatter _formatter = NetworkMessageFormatter.Shared;
    private readonly SharmProcessId _globalId;

    private ISharmIpc _sharmIpc = new Dummy();

    public SharmComunicator(SharmProcessId globalId, Action<string, Exception> errorHandler)
    {
        _globalId = globalId;
        _errorHandler = errorHandler;
    }
    #if DEBUG
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public static Func<ISharmIpc>? ConnectionFac { get; set; }
    #endif

    public void Dispose() => _sharmIpc.Dispose();

    public static bool MasterIpcReady(in SharmProcessId id)
    {
        try
        {
            using var mt = new Mutex(true, $"{id}SharmNet_MasterMutex", out bool created);

            if(!created) return true;

            #pragma warning disable MT1013
            mt.ReleaseMutex();
            #pragma warning restore MT1013
            return false;
        }
        catch (AbandonedMutexException)
        {
            return false;
        }
    }

    public event SharmMessageHandler? OnMessage;

    public void Connect()
    {
        #if DEBUG

        _sharmIpc = ConnectionFac is null ? new Connection(_globalId, _errorHandler) : ConnectionFac();

        #else
            _sharmIpc = new Connection(_globalId, _errorHandler);
        #endif
        _sharmIpc.OnMessage += OnSharmIpcOnOnMessage;
    }

    private void OnSharmIpcOnOnMessage(NetworkMessage message, ulong messageId, in Client processsId)
        => OnMessage?.Invoke(message, messageId, processsId);

    public bool Send(NetworkMessage msg, in Client target)
    {
        Client actualTarget = target.Value.Length switch
        {
            > 32 => throw new ArgumentOutOfRangeException(nameof(target), target, "Id Longer then 32 Chars"),
            < 32 => target.PadRight(32),
            _ => target
        };

        (var message, int lenght) = _formatter.WriteMessage(
            msg,
            i =>
            {
                var memory = MemoryPool<byte>.Shared.Rent(i + 64);

                Encoding.ASCII.GetBytes(actualTarget.Value, memory.Memory.Span);
                Encoding.ASCII.GetBytes(ProcessId.Value, memory.Memory.Span[31..]);

                return (memory, 63);
            });

        using var data = message;

        return _sharmIpc.Send(data.Memory[..lenght].ToArray());
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
        public void Dispose() { }

        public event SharmMessageHandler OnMessage
        {
            add { }
            remove { }
        }

        public bool Send(byte[] msg) => throw new InvalidOperationException("Ipc Not Started");
    }

    private sealed class Connection : ISharmIpc
    {
        private readonly NetworkMessageFormatter _messageFormatter = NetworkMessageFormatter.Shared;
        private readonly SharmIpc _sharmIpc;

        internal Connection(SharmProcessId globalId, Action<string, Exception> errorHandler)
            => _sharmIpc = new SharmIpc(
                globalId.Value,
                Handle,
                ExternalExceptionHandler: errorHandler,
                protocolVersion: SharmIpc.eProtocolVersion.V2);

        public void Dispose() => _sharmIpc.Dispose();

        public event SharmMessageHandler? OnMessage;

        public bool Send(byte[] msg) => _sharmIpc.RemoteRequestWithoutResponse(msg);

        private void Handle(ulong arg1, byte[] arg2)
        {
            string id = Encoding.ASCII.GetString(arg2, 0, 32).Trim();
            string from = Encoding.ASCII.GetString(arg2, 31, 32).Trim();

            if(id.StartsWith(Client.All.Value) || id == ProcessId)
                OnMessage?.Invoke(_messageFormatter.ReadMessage(arg2.AsMemory()[63..]), arg1, Client.From(from));
        }
    }
}

public sealed class SharmServer : IDataServer
{
    private readonly SharmComunicator _comunicator;

    public SharmServer(SharmProcessId uniqeName, Action<string, Exception> errorHandler)
    {
        _comunicator = new SharmComunicator(uniqeName, errorHandler);
        _comunicator.OnMessage += ComunicatorOnOnMessage;
    }

    public void Dispose() => _comunicator.Dispose();

    public event EventHandler<ClientConnectedArgs>? ClientConnected;

    public event EventHandler<ClientDisconnectedArgs>? ClientDisconnected;

    public event EventHandler<MessageFromClientEventArgs>? OnMessageReceived;

    public void Start() => _comunicator.Connect();

    public bool Send(in Client client, NetworkMessage message) => _comunicator.Send(message, client);

    private void ComunicatorOnOnMessage(NetworkMessage message, ulong messageId, in Client processId)
    {
        #pragma warning disable GU0011
        if(message.Type == SharmComunicatorMessage.RegisterClient)
        {
            _comunicator.Send(message, processId);
            ClientConnected?.Invoke(this, new ClientConnectedArgs(processId));

        }
        else if(message.Type == SharmComunicatorMessage.UnRegisterClient)
        {
            _comunicator.Send(message, processId);
            ClientDisconnected?.Invoke(this, new ClientDisconnectedArgs(processId, DisconnectReason.Normal));

        }
        else
        {
            OnMessageReceived?.Invoke(this, new MessageFromClientEventArgs(message, processId));

        }
        #pragma warning restore GU0011
    }
}

public sealed class SharmClient : IDataClient, IDisposable
{
    private readonly SharmComunicator _comunicator;

    public SharmClient(SharmProcessId uniqeName, Action<string, Exception> errorHandler)
    {
        _comunicator = new SharmComunicator(uniqeName, errorHandler);
        _comunicator.OnMessage += ComunicatorOnOnMessage;
    }

    public bool Connect()
    {
        _comunicator.Connect();

        return Send(NetworkMessage.Create(SharmComunicatorMessage.RegisterClient.Value));
    }

    public event EventHandler<ClientConnectedArgs>? Connected;
    public event EventHandler<ClientDisconnectedArgs>? Disconnected;
    public event EventHandler<MessageFromServerEventArgs>? OnMessageReceived;

    public bool Send(NetworkMessage msg)
        => _comunicator.Send(msg, Client.All);

    public void Dispose() => _comunicator.Dispose();

    private void ComunicatorOnOnMessage(NetworkMessage message, ulong messageid, in Client processsid)
    {
        if(message.Type == SharmComunicatorMessage.RegisterClient)
        {
            Connected?.Invoke(this, new ClientConnectedArgs(processsid));
        }
        else if(message.Type == SharmComunicatorMessage.UnRegisterClient)
        {
            Disconnected?.Invoke(this, new ClientDisconnectedArgs(processsid, DisconnectReason.Normal));
            Dispose();
        }
        else
        {
            OnMessageReceived?.Invoke(this, new MessageFromServerEventArgs(message));
        }
    }

    public void Disconnect()
        => _comunicator.Send(NetworkMessage.Create(SharmComunicatorMessage.UnRegisterClient.Value), Client.All);
}