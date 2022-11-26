using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Servicemnager.Networking.Data;

public sealed class MessageBuffer
{
    private readonly List<Memory<byte>> _incomming = new();
    private readonly NetworkMessageFormatter _messageFormatter;
    private readonly MemoryPool<byte> _pool;

    public MessageBuffer(MemoryPool<byte> pool)
    {
        _pool = pool;
        _messageFormatter = new NetworkMessageFormatter(pool);
    }

    public NetworkMessage? AddBuffer(Memory<byte> buffer)
    {
        if(_incomming.Count == 0 && !_messageFormatter.HasHeader(buffer))
            throw new InvalidOperationException("Incomming Message has no header");

        if(_incomming.Count != 0 && buffer.Length < NetworkMessageFormatter.End.Length)
        {
            _incomming.Add(buffer);

            var temp = Merge();
            using var merge = temp.Memory;

            if(_messageFormatter.HasTail(merge.Memory))
                return _messageFormatter.ReadMessage(merge.Memory);

            _incomming.Add(merge.Memory[..temp.Lenght].ToArray());

            return null;
        }

        if(_incomming.Count == 0 && _messageFormatter.HasTail(buffer))
        {
            _incomming.Add(buffer);
            var merge = Merge();
            using var data = merge.Memory;

            return _messageFormatter.ReadMessage(data.Memory);
        }

        if(_messageFormatter.HasTail(buffer))
        {
            _incomming.Add(buffer);
            var merge = Merge();
            using var data = merge.Memory;

            return _messageFormatter.ReadMessage(data.Memory);
        }

        _incomming.Add(buffer);

        return null;
    }

    private (IMemoryOwner<byte> Memory, int Lenght) Merge()
    {
        int minLenght = _incomming.Sum(a => a.Length);
        var data = _pool.Rent(minLenght);

        var start = 0;

        try
        {
            foreach (var bytese in _incomming)
            {
                bytese.CopyTo(data.Memory[start..]);
                start += bytese.Length;
            }

            _incomming.Clear();

            return (data, minLenght);
        }
        catch
        {
            data.Dispose();

            throw;
        }
    }
}