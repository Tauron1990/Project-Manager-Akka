using System.Net.Sockets;

namespace Tauron.Servicemnager.Networking.Data;

public interface IMessageStream : IDisposable
{
    bool DataAvailable { get; }
    
    Stream ReadStream { get; }

    bool Connected();
}