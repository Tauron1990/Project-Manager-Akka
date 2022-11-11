using JetBrains.Annotations;

namespace Tauron.Features;

[PublicAPI]
#pragma warning disable AV1564
public sealed record EventSubscribe(bool Watch, Type Event);