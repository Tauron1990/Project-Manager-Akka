using System.IO;

namespace Tauron.Application.AkkaNode.Services.FileTransfer;

public sealed class StreamData : ITransferData
{
    private readonly Stream _stream;

    public StreamData(Stream stream) => _stream = stream;
    public static ITransferData Null { get; } = new StreamData(Stream.Null);

    public void Dispose() => _stream.Dispose();
    public int Read(byte[] buffer, in int offset, in int count) => _stream.Read(buffer, offset, count);

    public void Write(byte[] buffer, in int offset, in int count) => _stream.Write(buffer, offset, count);
}