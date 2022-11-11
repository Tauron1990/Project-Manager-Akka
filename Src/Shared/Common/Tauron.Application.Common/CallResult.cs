using JetBrains.Annotations;

namespace Tauron;

[PublicAPI]
public abstract record CallResult<TType>(bool IsOk);