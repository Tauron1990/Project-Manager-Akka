using System;
using System.Buffers;
using Servicemnager.Networking.Data;
using tiesky.com;

namespace Servicemnager.Networking.IPC
{
    public sealed class SharmComunicator : IDisposable
    {
        public static Guid ProcessId = Guid.NewGuid();

        private readonly SharmIpc _sharmIpc;
        private readonly MessageBuffer _messageBuffer;

        public SharmComunicator(string globalId)
        {
            globalId = "Global/" + globalId;
            _sharmIpc = new SharmIpc(globalId, Handle, protocolVersion: SharmIpc.eProtocolVersion.V2, externalProcessing:true);
            _messageBuffer = new MessageBuffer(MemoryPool<byte>.Shared);
        }

        private void Handle(ulong arg1, byte[] arg2)
        {
            
        }

        public void Dispose() => _sharmIpc.Dispose();
    }
}