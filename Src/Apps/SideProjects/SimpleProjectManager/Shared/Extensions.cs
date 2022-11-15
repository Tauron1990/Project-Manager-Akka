using Tauron.Operations;
using Vogen;

namespace SimpleProjectManager.Shared;

public static class Extensions
{
    public static TResult ThrowIfFail<TResult>(this string? error, Func<TResult> resultFactory)
    {
        if(string.IsNullOrWhiteSpace(error))
            return resultFactory();

        throw new InvalidOperationException(error);
    }

    public static void ThrowIfFail(this string? error)
    {
        if(string.IsNullOrWhiteSpace(error))
            return;

        throw new InvalidOperationException(error);
    }

    public static TResult ThrowIfFail<TResult>(this in SimpleResult error, Func<TResult> resultFactory)
    {
        if(error.IsError())
            throw error.GetException();

        return resultFactory();
    }

    public static void ThrowIfFail(this in SimpleResult error)
    {
        if(error.IsError())
            throw error.GetException();
    }
    
    public static Validation ValidateNotNullOrEmpty(this string value, string name)
        => string.IsNullOrWhiteSpace(value) ? Validation.Invalid($"The Value from {name} is null or Empty") : Validation.Ok;
}