using System.Threading;
using JetBrains.Annotations;

namespace Tauron;

[PublicAPI]
public sealed record CopyFromArguments(long TotalLength = -1, int BufferSize = 4096, ProgressChange? ProgressChangeCallback = null, CancellationToken StopEvent = default);