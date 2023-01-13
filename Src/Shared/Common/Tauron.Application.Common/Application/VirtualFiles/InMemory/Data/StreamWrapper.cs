using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public class StreamWrapper : Stream
{
    private readonly FileAccess _access;
    private readonly MemoryStream _memoryStream;
    private readonly Action _modifyAction;

    private bool _modify;

    public StreamWrapper(MemoryStream memoryStream, FileAccess access, Action modifyAction)
    {
        _memoryStream = memoryStream;
        memoryStream.Seek(0, SeekOrigin.Begin);
        _access = access;
        _modifyAction = modifyAction;
    }

    public override bool CanTimeout => _memoryStream.CanTimeout;

    public override int WriteTimeout
    {
        get => _memoryStream.WriteTimeout;
        set => _memoryStream.WriteTimeout = value;
    }

    public override int ReadTimeout
    {
        get => _memoryStream.ReadTimeout;
        set => _memoryStream.ReadTimeout = value;
    }

    public override bool CanRead => _access.HasFlag(FileAccess.Read);

    public override bool CanSeek => _memoryStream.CanSeek;

    public override bool CanWrite => _access.HasFlag(FileAccess.Write);

    public override long Length => _memoryStream.Length;

    public override long Position
    {
        get => _memoryStream.Position;
        set => _memoryStream.Position = value;
    }

    protected override void Dispose(bool disposing)
    {
        if(_modify) _modifyAction();
    }

    public override void Flush()
        => _memoryStream.Flush();

    public override int Read(byte[] buffer, int offset, int count)
        => _memoryStream.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin)
        => _memoryStream.Seek(offset, origin);

    public override void SetLength(long value)
    {
        _modify = true;
        _memoryStream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _modify = true;
        _memoryStream.Write(buffer, offset, count);
    }

    public override int Read(Span<byte> buffer)
        => _memoryStream.Read(buffer);

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        _modify = true;
        _memoryStream.Write(buffer);
    }

    public override int ReadByte()
        => _memoryStream.ReadByte();

    public override void WriteByte(byte value)
    {
        _modify = true;
        _memoryStream.WriteByte(value);
    }

    public override void CopyTo(Stream destination, int bufferSize)
        => _memoryStream.CopyTo(destination, bufferSize);

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        => _memoryStream.CopyToAsync(destination, bufferSize, cancellationToken);

    public override Task FlushAsync(CancellationToken cancellationToken)
        => _memoryStream.FlushAsync(cancellationToken);

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new())
        => _memoryStream.ReadAsync(buffer, cancellationToken);

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => _memoryStream.ReadAsync(buffer, offset, count, cancellationToken);

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        _modify = true;

        return _memoryStream.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new())
    {
        _modify = true;

        return _memoryStream.WriteAsync(buffer, cancellationToken);
    }
}