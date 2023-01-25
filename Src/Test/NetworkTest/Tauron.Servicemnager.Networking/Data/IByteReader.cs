namespace Tauron.Servicemnager.Networking.Data;

public interface IByteReader : IDisposable
{
    ValueTask<int> ReadAsync(Memory<byte> mem, CancellationToken token);

    public static IByteReader Stream(Stream stream)
        => new StreamWrapper(stream);
}

file sealed class StreamWrapper : IByteReader
{
    private readonly Stream _stream;

    internal StreamWrapper(Stream stream)
        => _stream = stream;

    public ValueTask<int> ReadAsync(Memory<byte> mem, CancellationToken token)
        => _stream.ReadAsync(mem, token);

    public void Dispose()
        => _stream.Dispose();
}