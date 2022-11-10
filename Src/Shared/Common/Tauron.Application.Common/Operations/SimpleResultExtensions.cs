using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Tauron.Operations;

[PublicAPI]
public static class SimpleResultExtensions
{
    public static TValue Match<TValue, TOutcome>(this in SimpleResult<TOutcome> result, Func<TOutcome, TValue> success, Func<Error, TValue> onError)
        => result.Error is null ? success(result.OutCome!) : onError(result.Error.Value);

    public static async ValueTask<TValue> MatchAsync<TValue, TOutcome>(
        this SimpleResult<TOutcome> result,
        Func<TOutcome, ValueTask<TValue>> success,
        Func<Error, ValueTask<TValue>> onError)
        => result.Error is null 
            ? await success(result.OutCome!).ConfigureAwait(false) 
            : await onError(result.Error.Value).ConfigureAwait(false);

    public static bool IsError(this in SimpleResult result)
        => result.Error is not null;
    
    public static bool IsSuccess(this in SimpleResult result)
        => result.Error is null;

    public static Exception GetException(this in SimpleResult result)
    {
        if(result.Error is null)
            throw new ArgumentNullException(nameof(result), "Result error is null");

        return new InvalidOperationException(result.Error.Value.Info ?? result.Error.Value.Code);
    }
    
    public static string GetErrorString(this in SimpleResult result)
    {
        if(result.Error is null)
            throw new ArgumentNullException(nameof(result), "Result error is null");

        return result.Error.Value.Info ?? result.Error.Value.Code;
    }
}