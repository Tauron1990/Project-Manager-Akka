using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Channels;

#pragma warning disable EPS02

namespace Tauron.Servicemnager.Networking.Data;

public sealed class MessageReader<TMessage> : IDisposable
    where TMessage : class
{
    private readonly ErrorTracker _errorTracker;
    private readonly IMessageStream _messageStream;
    private readonly INetworkMessageFormatter<TMessage> _formatter;
    private readonly Pipe _pipe;
    private readonly long _forceAdvanceThreshold;

    public MessageReader(IMessageStream messageStream, INetworkMessageFormatter<TMessage> formatter, PipeOptions? pipeOptions = null)
    {
        _errorTracker = new ErrorTracker(GetType().Name);
        
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
                try
                {
                    if(await ActualRead(pipeReader, channel, messages, token).ConfigureAwait(false))
                        break;
                }
                catch (Exception ex)
                {
                    _errorTracker.AddException(ex);
                }
            }
        }
        catch (Exception ex)
        {
            await pipeReader.CompleteAsync(ex).ConfigureAwait(false);

            throw;
        }
        await pipeReader.CompleteAsync().ConfigureAwait(false);
    }

    private async ValueTask<bool> ActualRead(PipeReader pipeReader, ChannelWriter<TMessage> channel, List<TMessage> messages, CancellationToken token)
    {
        ReadResult read = await pipeReader.ReadAsync(token).ConfigureAwait(false);

        if(ReadIsCompletedOrEmpty(read)) return true;

        var buffer = read.Buffer;

        ReadBuffer(ref buffer, messages, out SequencePosition consumed, out SequencePosition exaimed);

        foreach (TMessage message in messages)
            await channel.WriteAsync(message, token).ConfigureAwait(false);
        messages.Clear();

        pipeReader.AdvanceTo(consumed, exaimed);


        return ReadIsCompleted(read);
    }

    private static bool ReadIsCompleted(ReadResult read)
        => read.IsCanceled || read.IsCompleted;

    private static bool ReadIsCompletedOrEmpty(ReadResult read)
        => (read.IsCanceled || read.IsCompleted) && read.Buffer.IsEmpty;

    private void ReadBuffer(ref ReadOnlySequence<byte> buffer, ICollection<TMessage> messages, out SequencePosition consumed, out SequencePosition exaimed)
    {
        bool isMax = _forceAdvanceThreshold <= buffer.Length;

        consumed = buffer.Start;
        exaimed = buffer.Start;
        
        while (true)
        {
            if(buffer.IsEmpty) return;

            TMessage? msg = TryRead(ref buffer, out exaimed);

            if(msg is not null)
                messages.Add(msg);
            else
            {
                if(isMax)
                {
                    consumed = buffer.End;
                    exaimed = buffer.End;
                }
                    
                break;
            }

            if(exaimed.Equals(buffer.End))
                break;
            
            buffer = buffer.Slice(buffer.Start, exaimed);
            consumed = buffer.Start;
        }
    }

    private TMessage? TryRead(ref ReadOnlySequence<byte> buffer, out SequencePosition exaimed)
    {
        var reader = new SequenceReader<byte>(buffer);

        try
        {
            if(!reader.TryReadTo(out ReadOnlySequence<byte> messageData, _formatter.Header.Span))
                return null;

            return !reader.TryReadTo(out messageData, _formatter.Tail.Span) ? null : _formatter.ReadMessage(messageData);
        }
        finally
        {
            exaimed = reader.Position;
        }
    }

    public void Dispose()
    {
        _messageStream.Dispose();
        _errorTracker.Dispose();
    }
}