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
        Murmur32? murmur = MurmurHash.Create32();
        byte[] bytes = Encoding.UTF8.GetBytes(logEvent.Message);
        byte[] hash = murmur.ComputeHash(bytes);
        var numericHash = BitConverter.ToUInt32(hash, 0);

        #pragma warning disable MA0011
        builder.Append($"{numericHash:x8}");
        #pragma warning restore MA0011
    }
}