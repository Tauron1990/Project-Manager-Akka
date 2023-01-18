using System.Buffers;
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

    public TMessage? AddBuffer(Memory<byte> buffer)
        => AddBuffer(new OrphanOwner(buffer));
    
    public TMessage? AddBuffer(IMemoryOwner<byte> bufferOwner)
    {
        var buffer = bufferOwner.Memory;
        
        if(_incomming.Count == 0 && !_messageFormatter.HasHeader(buffer))
            throw new InvalidOperationException("Incomming Message has no header");

        if(_incomming.Count != 0 && buffer.Length < NetworkMessageFormatter.End.Length)
        {
            _incomming.Add(bufferOwner);

            var temp = Merge();
            var merge = temp.Memory;

            if(_messageFormatter.HasTail(merge.Memory))
            {
                using(merge)
                    return _messageFormatter.ReadMessage(merge.Memory);
            }

            _incomming.Add(new LinkedOwner(merge, merge.Memory[..temp.Lenght]));

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