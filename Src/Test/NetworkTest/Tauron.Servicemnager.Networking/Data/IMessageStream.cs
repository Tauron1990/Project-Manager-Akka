
namespace Tauron.Servicemnager.Networking.Data;

public interface IMessageStream : IDisposable
{
    bool DataAvailable { get; }
    
    IByteReader ReadStream { get; }

    ValueTask<bool> Connected();
}