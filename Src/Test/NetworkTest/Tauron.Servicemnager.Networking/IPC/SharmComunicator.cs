using System.Buffers;
using System.Text;
using JetBrains.Annotations;
using Tauron.Servicemnager.Networking.Data;
using Tauron.Servicemnager.Networking.IPC.Core;

namespace Tauron.Servicemnager.Networking.IPC;

[PublicAPI]
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
            using var mt = new Mutex(initiallyOwned: true, $"{id}SharmNet_MasterMutex", out bool created);

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

    #pragma warning disable MA0046
    public event SharmMessageHandler? OnMessage;
    #pragma warning restore MA0046

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
        #pragma warning disable MA0046
        event SharmMessageHandler OnMessage;
        #pragma warning restore MA0046

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
                externalExceptionHandler: errorHandler,
                protocolVersion: SharmIpc.EProtocolVersion.V2);

        public void Dispose() => _sharmIpc.Dispose();

        public event SharmMessageHandler? OnMessage;

        public bool Send(byte[]? msg) => _sharmIpc.RemoteRequestWithoutResponse(msg);

        private void Handle(ulong arg1, byte[]? arg2)
        {
            if(arg2 is null) return;
            
            string id = Encoding.ASCII.GetString(arg2, 0, 32).Trim();
            string from = Encoding.ASCII.GetString(arg2, 31, 32).Trim();

            if(id.StartsWith(Client.All.Value, StringComparison.Ordinal) || id == ProcessId)
                OnMessage?.Invoke(_messageFormatter.ReadMessage(arg2.AsMemory()[63..]), arg1, Client.From(from));
        }
    }
}