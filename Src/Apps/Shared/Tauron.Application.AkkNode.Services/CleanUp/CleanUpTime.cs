using System;
using JetBrains.Annotations;
using UnitsNet;

namespace Tauron.Application.AkkaNode.Services.CleanUp;

public sealed record CleanUpTime([UsedImplicitly]string Id, Duration Interval, DateTime Last)
{
    public CleanUpTime()
        : this(string.Empty, Duration.Zero, DateTime.MinValue) { }
}