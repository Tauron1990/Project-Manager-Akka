using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace Tauron.Application
{
    [PublicAPI]
    public class Crc32
    {
        private readonly uint[] _table;

        public Crc32()
        {
            const uint poly = 0xedb88320;
            _table = new uint[256];
            for (uint tableIndex = 0; tableIndex < _table.Length; ++tableIndex)
            {
                var temp = tableIndex;
                for (var calc = 8; calc > 0; --calc)
                    if ((temp & 1) == 1)
                        temp = (temp >> 1) ^ poly;
                    else
                        temp >>= 1;

                _table[tableIndex] = temp;
            }
        }

        [DebuggerStepThrough]
        public uint ComputeChecksum(byte[] bytes, int count)
        {
            var crc = 0xffffffff;
            foreach (var deta in bytes.Take(count))
            {
                var index = (byte)((crc & 0xff) ^ deta);
                crc = (crc >> 8) ^ _table[index];
            }

            return ~crc;
        }

        #pragma warning disable AV1130
        public byte[] ComputeChecksumBytes(byte[] bytes, int count)
            #pragma warning restore AV1130
            => BitConverter.GetBytes(ComputeChecksum(bytes, count));
    }

    /// <summary>
    ///     Encapsulates a <see cref="System.IO.Stream" /> to calculate the CRC32 checksum on-the-fly as data passes through.
    /// </summary>
    [PublicAPI]
    public class CrcStream : Stream
    {
        private static readonly uint[] Table = GenerateTable();

        private uint _readCrc = 0xFFFFFFFF;

        private uint _writeCrc = 0xFFFFFFFF;

        /// <summary>
        ///     Encapsulate a <see cref="System.IO.Stream" />.
        /// </summary>
        /// <param name="stream">The stream to calculate the checksum for.</param>
        public CrcStream(Stream stream) => Stream = stream;

        /// <summary>
        ///     Gets the underlying stream.
        /// </summary>
        public Stream Stream { get; }

        public override bool CanRead => Stream.CanRead;

        public override bool CanSeek => Stream.CanSeek;

        public override bool CanWrite => Stream.CanWrite;

        public override long Length => Stream.Length;

        public override long Position
        {
            get => Stream.Position;
            set => Stream.Position = value;
        }

        /// <summary>
        ///     Gets the CRC checksum of the data that was read by the stream thus far.
        /// </summary>
        public uint ReadCrc => _readCrc ^ 0xFFFFFFFF;

        /// <summary>
        ///     Gets the CRC checksum of the data that was written to the stream thus far.
        /// </summary>
        public uint WriteCrc => _writeCrc ^ 0xFFFFFFFF;

        public override void Flush()
        {
            Stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin) => Stream.Seek(offset, origin);

        public override void SetLength(long value)
        {
            Stream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            count = Stream.Read(buffer, offset, count);
            _readCrc = CalculateCrc(_readCrc, buffer, offset, count);

            return count;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Stream.Write(buffer, offset, count);

            _writeCrc = CalculateCrc(_writeCrc, buffer, offset, count);
        }

        [DebuggerStepThrough]
        private static uint CalculateCrc(uint crc, byte[] buffer, int offset, int count)
        {
            unchecked
            {
                #pragma warning disable AV1522
                for (int index = offset, end = offset + count; index < end; index++)
                    #pragma warning restore AV1522
                    crc = (crc >> 8) ^ Table[(crc ^ buffer[index]) & 0xFF];
            }

            return crc;
        }

        #pragma warning disable AV1130
        private static uint[] GenerateTable()
            #pragma warning restore AV1130
        {
            unchecked
            {
                uint[] table = new uint[256];

                const uint poly = 0xEDB88320;
                for (uint tableIndex = 0; tableIndex < table.Length; tableIndex++)
                {
                    var crc = tableIndex;
                    for (var calc = 8; calc > 0; calc--)
                        if ((crc & 1) == 1)
                            crc = (crc >> 1) ^ poly;
                        else
                            crc >>= 1;

                    table[tableIndex] = crc;
                }

                return table;
            }
        }

        /// <summary>
        ///     Resets the read and write checksums.
        /// </summary>
        public void ResetChecksum()
        {
            _readCrc = 0xFFFFFFFF;
            _writeCrc = 0xFFFFFFFF;
        }

        protected override void Dispose(bool disposing)
        {
            Stream.Dispose();
            base.Dispose(disposing);
        }
    }
}