using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace Tauron.Application;

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
            uint temp = tableIndex;
            for (var calc = 8; calc > 0; --calc)
                if((temp & 1) == 1)
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
        foreach (byte deta in bytes.Take(count))
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