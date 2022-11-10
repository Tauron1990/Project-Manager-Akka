using System;

namespace Tauron.Operations;

public readonly record struct Error(string? Info, string Code)
{
    public Error(Exception ex)
        : this(ex.Message, ex.HResult.ToString()) { }

    public static implicit operator Error(string code)
        => new(null, code);

    public Exception CreateException()
        => new InvalidOperationException(Info ?? Code);
}