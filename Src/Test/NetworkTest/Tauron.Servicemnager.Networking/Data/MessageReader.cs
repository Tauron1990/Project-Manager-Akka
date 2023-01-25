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

    public async Task ReadAsync(ChannelWriter<TMessage> channel, CancellationToken token)
    {
        try
        {
            await Task.WhenAll(DoWrite(_pipe.Writer, token), DoRead(_pipe.Reader, channel, token)).ConfigureAwait(false);
        }
        finally
        {
            channel.Complete();
        }
    }

    private async Task DoWrite(PipeWriter writer, CancellationToken token)
    {
        try
        {
            while (_messageStream.Connected() && !token.IsCancellationRequested)
            {
                if(!_messageStream.DataAvailable)
                {
                    await Task.Delay(1000, token).ConfigureAwait(false);
                    continue;
                }
                
                var mem = writer.GetMemory();

                int num = await _messageStream.ReadStream.ReadAsync(mem, token).ConfigureAwait(false);

                writer.Advance(num);

                FlushResult flush = await writer.FlushAsync(token).ConfigureAwait(false);

                if(flush.IsCanceled || flush.IsCompleted) break;
            }
        }
        catch (Exception ex)
        {
            
            await writer.CompleteAsync(ex).ConfigureAwait(false);
            throw;
        }

        await writer.CompleteAsync().ConfigureAwait(false);
    }


    private async Task DoRead(PipeReader pipeReader, ChannelWriter<TMessage> channel, CancellationToken token)
    {
        var messages = new List<TMessage>();
        
        try
        {
            while (true)
            {
                ReadResult read = await pipeReader.ReadAsync(token).ConfigureAwait(false);

                if(ReadIsCompletedOrEmpty(read)) break;

                var buffer = read.Buffer;

                ReadBuffer(buffer, messages, out SequencePosition end);

                foreach (TMessage message in messages)
                    await channel.WriteAsync(message, token).ConfigureAwait(false);
                messages.Clear();
                
                pipeReader.AdvanceTo(buffer.Start, end);
                
                if(ReadIsCompleted(read)) break;
            }
        }
        catch (Exception ex)
        {
            await pipeReader.CompleteAsync(ex).ConfigureAwait(false);

            throw;
        }
        await pipeReader.CompleteAsync().ConfigureAwait(false);
    }

    private static bool ReadIsCompleted(ReadResult read)
        => read.IsCanceled || read.IsCompleted;

    private static bool ReadIsCompletedOrEmpty(ReadResult read)
        => (read.IsCanceled || read.IsCompleted) && read.Buffer.IsEmpty;

    private void ReadBuffer(ReadOnlySequence<byte> buffer, ICollection<TMessage> messages, out SequencePosition end)
    {
        end = buffer.Start;

        while (true)
        {
            if(buffer.IsEmpty) return;

            TMessage? msg = TryRead(buffer, out end);

            if(msg is not null)
                messages.Add(msg);

            buffer = buffer.Slice(end);
        }
    }

    private TMessage? TryRead(in ReadOnlySequence<byte> buffer, out SequencePosition end)
    {
        var reader = new SequenceReader<byte>(buffer);
        end = reader.Position;

        if(!reader.TryReadTo(out ReadOnlySequence<byte> messageData, _formatter.Header.Span))
            return null;

        end = reader.Position;
        
        if(!reader.TryReadTo(out messageData, _formatter.Tail.Span))
            return null;

        end = reader.Position;
        return _formatter.ReadMessage(messageData);


    }

    public void Dispose()
        => _messageStream.Dispose();
}