using System;
using tiesky.com;

namespace Servicemnager.Networking.IPC
{
    public sealed class SharmComunicator : IDisposable
    {
        public static Guid ProcessId = Guid.NewGuid();

        private readonly SharmIpc _sharmIpc;

        public SharmComunicator(string globalId)
        {
            globalId = "Global/" + globalId;
            _sharmIpc = new SharmIpc(globalId, Handle, protocolVersion: SharmIpc.eProtocolVersion.V2);
        }

        private void Handle(ulong arg1, byte[] arg2)
        {
            
        }

        public void Dispose() => _sharmIpc.Dispose();
    }
}