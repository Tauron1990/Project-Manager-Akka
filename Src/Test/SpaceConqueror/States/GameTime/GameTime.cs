using System.Diagnostics;
using JetBrains.Annotations;

namespace SpaceConqueror.States.GameTime;

public sealed record GameTime(TimeSpan Current, TimeSpan LastUpdate) : IState
{
    [UsedImplicitly]
    public GameTime()
        : this(TimeSpan.Zero, TimeSpan.Zero){}

    public Stopwatch Stopwatch { get; } = new();
}