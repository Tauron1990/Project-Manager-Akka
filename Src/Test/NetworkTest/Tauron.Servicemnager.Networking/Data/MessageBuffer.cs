using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Tauron.Servicemnager.Networking.Data;

[PublicAPI]
public sealed class MessageBuffer<TMessage> : IDisposable
    where TMessage : class
{
    private readonly List<IMemoryOwner<byte>> _incomming = new();
    private readonly INetworkMessageFormatter<TMessage> _messageFormatter;
    private readonly MemoryPool<byte> _pool;

    public MessageBuffer(INetworkMessageFormatter<TMessage> messageFormatter, MemoryPool<byte> pool)
    {
        _messageFormatter = messageFormatter;
        _pool = pool;
    }

    public TMessage? AddBuffer(Memory<byte> buffer, int lenght = -1)
        => AddBuffer(new OrphanOwner(buffer), lenght);
    
    public TMessage? AddBuffer(IMemoryOwner<byte> bufferOwner, int msgLenght = -1)
    {
        if(msgLenght != -1)
            bufferOwner = new LinkedOwner(bufferOwner, bufferOwner.Memory[..msgLenght]);
            
        var buffer = bufferOwner.Memory;

        if(CheckHead(bufferOwner, buffer))
            return null;

        if(_incomming.Count != 0 && buffer.Length < _messageFormatter.TailLength)
        {
            _incomming.Add(bufferOwner);

            (var memory, int lenght) = Merge();

            if(_messageFormatter.HasTail(memory.Memory))
            {
                using(memory)
                    return _messageFormatter.ReadMessage(memory.Memory);
            }

            _incomming.Add(new LinkedOwner(memory, memory.Memory[..lenght]));

            return null;
        }

        if(_incomming.Count == 0 && _messageFormatter.HasTail(buffer))
        {
            _incomming.Add(bufferOwner);
            var merge = Merge();
            using var data = merge.Memory;

            return _messageFormatter.ReadMessage(data.Memory);
        }

        if(_messageFormatter.HasTail(buffer))
        {
            _incomming.Add(bufferOwner);
            var merge = Merge();
            using var data = merge.Memory;

            return _messageFormatter.ReadMessage(data.Memory);
        }

        _incomming.Add(bufferOwner);

        return null;
    }

    private bool CheckHead(IMemoryOwner<byte> bufferOwner, Memory<byte> buffer)
    {
        if(_incomming.Count != 0 || _messageFormatter.HasHeader(buffer))
            return false;

        if(buffer.Length == _messageFormatter.HeaderLength)
            throw new InvalidOperationException("Incomming Message has no header");

        if(_incomming.Count == 0)
        {
            _incomming.Add(bufferOwner);

            return true;
        }

        var merged = Merge();

        if(merged.Lenght >= _messageFormatter.TailLength && !_messageFormatter.HasTail(merged.Memory.Memory))
            throw new InvalidOperationException("Incomming Message has no header");

        _incomming.Add(merged.Memory);

        return true;

    }

    private (IMemoryOwner<byte> Memory, int Lenght) Merge()
    {
        int minLenght = _incomming.Sum(a => a.Memory.Length);
        var data = _pool.Rent(minLenght);

        var start = 0;

        try
        {
            foreach (var bytese in _incomming)
            {
                bytese.Memory.CopyTo(data.Memory[start..]);
                start += bytese.Memory.Length;
            }

            _incomming.ForEach(m => m.Dispose());
            _incomming.Clear();

            return (data, minLenght);
        }
        catch
        {
            data.Dispose();

            throw;
        }
    }
    
    private sealed class LinkedOwner : IMemoryOwner<byte>
    {
        private readonly IMemoryOwner<byte> _original;

        public Memory<byte> Memory { get; }

        internal LinkedOwner(IMemoryOwner<byte> original, Memory<byte> memory)
        {
            Memory = memory;
            if(original is LinkedOwner linkedOwner)
                _original = linkedOwner._original;
            else
                _original = original;
        }

        public void Dispose()
            => _original.Dispose();
    }
    
    private sealed class OrphanOwner : IMemoryOwner<byte>
    {
        public void Dispose()
        {
            
        }

        public Memory<byte> Memory { get; }

        internal OrphanOwner(Memory<byte> memory)
            => Memory = memory;
    }

    public void Dispose()
    {
        _incomming.ForEach(d => d.Dispose());
        _incomming.Clear();
    }
}