using System;
using System.Threading;
using UnitsNet;

namespace Tauron.Application.AkkaNode.Services.Reporting.Commands;

public sealed record ApiParameter(Duration? Timeout, CancellationToken CancellationToken, Action<string> Messages)
{
    public ApiParameter(Duration? timeout)
        : this(timeout, CancellationToken.None, _ => { }) { }

    public ApiParameter(Duration? timeout, Action<string> messages)
        : this(timeout, CancellationToken.None, messages) { }

    public ApiParameter(Duration? timeout, CancellationToken token)
        : this(timeout, token, _ => { }) { }
}