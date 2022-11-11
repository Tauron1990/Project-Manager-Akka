using JetBrains.Annotations;

namespace Tauron.Features;

public sealed record SendEvent(object Event, Type EventType)
{
    [PublicAPI]
    public static SendEvent Create<TType>(TType evt)
        where TType : notnull
        => new(evt, typeof(TType));
}