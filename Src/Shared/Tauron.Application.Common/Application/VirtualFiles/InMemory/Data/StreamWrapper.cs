using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IO;

namespace Tauron.Application.VirtualFiles.InMemory.Data
{
    public class StreamWrapper : Stream
    {
        private readonly RecyclableMemoryStream _memoryStream;

        public StreamWrapper(RecyclableMemoryStream memoryStream)
            => _memoryStream = memoryStream;
        
        public override void Flush()
            => _memoryStream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
            => _memoryStream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin)
            => _memoryStream.Seek(offset, origin);

        public override void SetLength(long value)
            => _memoryStream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count)
            => _memoryStream.Write(buffer, offset, count);

        public override int Read(Span<byte> buffer)
            => _memoryStream.Read(buffer);

        public override void Write(ReadOnlySpan<byte> buffer)
            => _memoryStream.Write(buffer);

        public override int ReadByte()
            => _memoryStream.ReadByte();

        public override bool CanTimeout => _memoryStream.CanTimeout;

        public override void WriteByte(byte value)
            => _memoryStream.WriteByte(value);

        public override int WriteTimeout
        {
            get => _memoryStream.WriteTimeout;
            set => _memoryStream.WriteTimeout = value;
        }

        public override void CopyTo(Stream destination, int bufferSize)
            => _memoryStream.CopyTo(destination, bufferSize);

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            => _memoryStream.CopyToAsync(destination, bufferSize, cancellationToken);

        public override int ReadTimeout
        {
            get => _memoryStream.ReadTimeout; 
            set => _memoryStream.ReadTimeout = value;
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
            => _memoryStream.FlushAsync(cancellationToken);

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new())
            => _memoryStream.ReadAsync(buffer, cancellationToken);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => _memoryStream.ReadAsync(buffer,offset, count, cancellationToken);

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => _memoryStream.WriteAsync(buffer, offset, count, cancellationToken);

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new())
            => _memoryStream.WriteAsync(buffer, cancellationToken);

        public override bool CanRead => _memoryStream.CanRead;

        public override bool CanSeek => _memoryStream.CanSeek;

        public override bool CanWrite => _memoryStream.CanWrite;

        public override long Length => _memoryStream.Length;

        public override long Position
        {
            get => _memoryStream.Position;
            set => _memoryStream.Position = value;
        }
    }
}