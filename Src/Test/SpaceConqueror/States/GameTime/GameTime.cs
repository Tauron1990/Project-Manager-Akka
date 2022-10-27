using System.Diagnostics;
using JetBrains.Annotations;

namespace SpaceConqueror.States.GameTime;

public sealed record GameTime(TimeSpan Current, TimeSpan LastUpdate, Stopwatch Stopwatch) : IState
{
    [UsedImplicitly]
    public GameTime()
        : this(TimeSpan.Zero, TimeSpan.Zero, Stopwatch.StartNew()){}
}