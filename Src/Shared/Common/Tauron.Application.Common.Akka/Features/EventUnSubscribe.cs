using JetBrains.Annotations;

namespace Tauron.Features;

[PublicAPI]
public sealed record EventUnSubscribe(Type Event);