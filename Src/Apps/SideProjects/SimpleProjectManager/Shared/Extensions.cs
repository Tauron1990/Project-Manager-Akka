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
}