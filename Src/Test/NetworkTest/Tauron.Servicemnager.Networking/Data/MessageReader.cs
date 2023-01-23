using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Channels;

namespace Tauron.Servicemnager.Networking.Data;

public class MessageReader<TMessage> : IDisposable
    where TMessage : class
{
    private readonly IMessageStream _messageStream;
    private readonly INetworkMessageFormatter<TMessage> _formatter;
    private readonly Pipe _pipe;

    public MessageReader(IMessageStream messageStream, INetworkMessageFormatter<TMessage> formatter, PipeOptions? pipeOptions = null)
    {
        _messageStream = messageStream;
        _formatter = formatter;

        _pipe = new Pipe(pipeOptions ?? PipeOptions.Default);
    }

    public Task ReadAsync(ChannelWriter<TMessage> channel, CancellationToken token)
        => Task.WhenAll(DoWrite(_pipe.Writer, token), DoRead(_pipe.Reader, channel, token));

    private async Task DoWrite(PipeWriter writer, CancellationToken token)
    {
        try
        {
            while (_messageStream.Connected() && !token.IsCancellationRequested)
            {
                var mem = writer.GetMemory();

                int num = await _messageStream.ReadStream.ReadAsync(mem, token).ConfigureAwait(false);

                writer.Advance(num);

                FlushResult flush = await writer.FlushAsync(token).ConfigureAwait(false);
                
                if(flush.IsCanceled || flush.IsCompleted) break;
            }
        }
        finally
        {
            await writer.CompleteAsync().ConfigureAwait(false);
        }
    }

    private async Task DoRead(PipeReader pipeReader, ChannelWriter<TMessage> channel, CancellationToken token)
    {
        try
        {
            while (true)
            {
                ReadResult read = await pipeReader.ReadAsync(token).ConfigureAwait(false);

                if(read.IsCanceled || read.IsCompleted) break;

                var buffer = read.Buffer;
                
                if(buffer.IsEmpty) continue;

                TMessage? msg = TryRead(buffer, out SequencePosition end);
                if(msg is null) continue;
                
                pipeReader.AdvanceTo(buffer.Start, end);
            }
        }
        finally
        {
            await pipeReader.CompleteAsync().ConfigureAwait(false):
        }
    }

    private TMessage? TryRead(in ReadOnlySequence<byte> buffer, out SequencePosition end)
    {
        
    }

    public void Dispose()
        => _messageStream.Dispose();
}