using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading.Channels;

namespace Tauron.Servicemnager.Networking.Data;

public class MessageReader<TMessage> : IDisposable
    where TMessage : class
{
    private readonly IMessageStream _messageStream;
    private readonly INetworkMessageFormatter<TMessage> _formatter;
    private readonly Pipe _pipe;
    private readonly long _forceAdvanceThreshold;

    public MessageReader(IMessageStream messageStream, INetworkMessageFormatter<TMessage> formatter, PipeOptions? pipeOptions = null)
    {
        _messageStream = messageStream;
        _formatter = formatter;

        PipeOptions actualOptions = pipeOptions ?? PipeOptions.Default;

        _forceAdvanceThreshold = actualOptions.PauseWriterThreshold;
        _pipe = new Pipe(actualOptions);
    }

    public async Task ReadAsync(ChannelWriter<TMessage> channel, CancellationToken token)
    {
        try
        {
            await Task.WhenAll(
                Task.Run(() => DoWrite(_pipe.Writer, token), token), 
                Task.Run(() => DoRead(_pipe.Reader, channel, token), token)).ConfigureAwait(false);
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
        catch(OperationCanceledException)
        {}
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

                ReadBuffer(ref buffer, messages, out SequencePosition end);

                foreach (TMessage message in messages)
                    await channel.WriteAsync(message, token).ConfigureAwait(false);
                messages.Clear();

                pipeReader.AdvanceTo(end);

                
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

    private void ReadBuffer(ref ReadOnlySequence<byte> buffer, ICollection<TMessage> messages, out SequencePosition end)
    {
        bool isMax = _forceAdvanceThreshold <= buffer.Length;

        end = buffer.Start;
        
        while (true)
        {
            if(buffer.IsEmpty) return;

            TMessage? msg = TryRead(buffer, end, isMax, out end);

            if(msg is not null)
                messages.Add(msg);
            else
                break;

            if(end.Equals(buffer.Start))
                break;
        }
    }

    private TMessage? TryRead(in ReadOnlySequence<byte> buffer, SequencePosition from, bool isMax, out SequencePosition end)
    {
        var reader = new SequenceReader<byte>(buffer);
        end = from;

        reader.Advance(from.GetInteger());
        
        if(!reader.TryReadTo(out ReadOnlySequence<byte> messageData, _formatter.Header.Span))
            return null;
        
        if(isMax)
            end = reader.Position;
        
        if(!reader.TryReadTo(out messageData, _formatter.Tail.Span))
            return null;

        end = reader.Position;
        return _formatter.ReadMessage(messageData);
    }

    public void Dispose()
        => _messageStream.Dispose();
}