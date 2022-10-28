using System.Diagnostics;

namespace SpaceConqueror.States.GameTime;

public sealed class GameTime : IState
{
    public TimeSpan Current { get; internal set; } = TimeSpan.Zero;
    
    public TimeSpan LastUpdate { get; internal set; } =TimeSpan.Zero;
    
    public Stopwatch Stopwatch { get; } = new();
}