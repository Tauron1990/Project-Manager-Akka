using System;

namespace Tauron.Operations;

public readonly record struct Error(string? Info, string Code)
{
    public static implicit operator Error(string code)
        => new(null, code);

    public static Error FromException(Exception e, IFormatProvider? provider = null) => new(e.Message, e.HResult.ToString(provider));

    public Exception CreateException()
        => new InvalidOperationException(Info ?? Code);
}