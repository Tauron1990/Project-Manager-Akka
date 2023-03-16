using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;

namespace Tauron;

/// <summary>
///     A static class for basic stream operations.
/// </summary>
[PublicAPI]
public static class StreamHelper
{
    /// <summary>
    ///     Copies the source stream into the current while reporting the progress.
    ///     The copying process is done in a separate thread, therefore the stream has to
    ///     support reading from a different thread as the one used for construction.
    ///     Nethertheless, the separate thread is synchronized with the calling thread.
    ///     The callback in arguments is called from the calling thread.
    /// </summary>
    /// <param name="target">The current stream</param>
    /// <param name="source">The source stream</param>
    /// <param name="arguments">The arguments for copying</param>
    /// <returns>The number of bytes actually copied.</returns>
    /// <exception cref="ArgumentNullException">Thrown if either target, source of arguments is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown if arguments.BufferSize is less than 128 or
    ///     arguments.ProgressChangeCallbackInterval is less than 0
    /// </exception>
    public static async Task<long> CopyFrom(this Stream target, Stream source, CopyFromArguments arguments)
    {
        ValidateArguments(arguments);

        byte[] buffer = ArrayPool<byte>.Shared.Rent(arguments.BufferSize);
        try
        {
            long totalLength = arguments.TotalLength;
            if(totalLength == -1 && source.CanSeek) totalLength = source.Length;

            long length = 0;

            do
            {
                var mem = buffer.AsMemory(0, arguments.BufferSize);
                int count = await source.ReadAsync(mem, arguments.StopEvent).ConfigureAwait(false);

                if(count == 0) break;

                await target.WriteAsync(mem, arguments.StopEvent).ConfigureAwait(false);

                length += count;

                if(arguments.StopEvent.IsCancellationRequested)
                    break;

                arguments.ProgressChangeCallback?.Invoke(count, totalLength);

            } while (!arguments.StopEvent.IsCancellationRequested);

            arguments.ProgressChangeCallback?.Invoke(length, totalLength);

            return length;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static void ValidateArguments(CopyFromArguments arguments)
    {
        #pragma warning disable EX005
        if(arguments.BufferSize < 128)
            throw new ArgumentOutOfRangeException(
                // ReSharper disable once NotResolvedInText
                nameof(arguments),
                arguments.BufferSize,
                "BufferSize has to be greater or equal than 128.");

        #pragma warning restore EX005
    }

    /// <summary>
    ///     Copies the source stream into the current
    /// </summary>
    /// <param name="stream">The current stream</param>
    /// <param name="source">The source stream</param>
    /// <param name="bufferSize">The size of buffer used for copying bytes</param>
    /// <returns>The number of bytes actually copied.</returns>
    public static long CopyFrom(this Stream stream, Stream source, int bufferSize = 4096)
    {
        int count;
        var buffer = new byte[bufferSize];
        long length = 0;

        while ((count = source.Read(buffer, 0, bufferSize)) != 0)
        {
            length += count;
            stream.Write(buffer, 0, count);
        }

        return length;
    }
}