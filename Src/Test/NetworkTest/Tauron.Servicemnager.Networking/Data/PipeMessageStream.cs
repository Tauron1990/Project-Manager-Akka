using System.IO.Pipes;

namespace Tauron.Servicemnager.Networking.Data;

public sealed class PipeMessageStream : IMessageStream
{
    private readonly PipeStream _pipeStream;

    public PipeMessageStream(PipeStream pipeStream) => _pipeStream = pipeStream;

    public void Dispose() => _pipeStream.Dispose();

    public bool DataAvailable => !_pipeStream.IsMessageComplete;
    public IByteReader ReadStream => IByteReader.Stream(_pipeStream);
    public async ValueTask<bool> Connected()
    {
        if(_pipeStream.IsConnected)
            return true;

        try
        {
            if(_pipeStream is NamedPipeServerStream serverStream)
            {
                await serverStream.WaitForConnectionAsync().ConfigureAwait(false);
                return true;
            }

            return false;
        }
        catch(ObjectDisposedException)
        {
            return false;
        }
    }
}