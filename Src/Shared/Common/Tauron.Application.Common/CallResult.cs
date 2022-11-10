using JetBrains.Annotations;

namespace Tauron;

[PublicAPI]
public abstract record CallResult(bool IsOk);