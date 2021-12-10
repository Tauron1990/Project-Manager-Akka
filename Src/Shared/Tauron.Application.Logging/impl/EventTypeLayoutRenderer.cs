using System;
using System.Diagnostics;
using System.Text;
using Murmur;
using NLog;
using NLog.LayoutRenderers;

namespace Tauron.Application.Logging.impl;

[DebuggerStepThrough]
[LayoutRenderer("event-type")]
public sealed class EventTypeLayoutRenderer : LayoutRenderer
{
    protected override void Append(StringBuilder builder, LogEventInfo logEvent)
    {
        var murmur = MurmurHash.Create32();
        var bytes = Encoding.UTF8.GetBytes(logEvent.Message);
        var hash = murmur.ComputeHash(bytes);
        var numericHash = BitConverter.ToUInt32(hash, 0);

        builder.Append($"{numericHash:x8}");
    }
}