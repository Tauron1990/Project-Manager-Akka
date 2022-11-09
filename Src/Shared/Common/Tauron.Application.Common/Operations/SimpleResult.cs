using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Tauron.Operations;

[PublicAPI]
public readonly record struct SimpleResult<TOutCome>(TOutCome? OutCome, Error? Error);

[PublicAPI]
public readonly record struct SimpleResult(Error? Error)
{
    public static SimpleResult Success() => new(null);

    public static SimpleResult<TResult> Success<TResult>(TResult result) => new(result, null);

    public static SimpleResult Failure(in Error error) => new(error);

    public static SimpleResult Failure(string error) => new(new Error(null, error));

    public static SimpleResult Failure(Exception error) => new(new Error(error));

    public static SimpleResult<TResult> Failure<TResult>(in Error error) => new(default, error);

    public static SimpleResult<TResult> Failure<TResult>(Exception error) => new(default, new Error(error));
}

[PublicAPI]
public static class SimpleResultExtensions
{
    public static TValue Match<TValue, TOutcome>(this in SimpleResult<TOutcome> result, Func<TOutcome, TValue> success, Func<Error, TValue> onError)
        => result.Error is null ? success(result.OutCome!) : onError(result.Error.Value);

    public static async ValueTask<TValue> MatchAsync<TValue, TOutcome>(
        this SimpleResult<TOutcome> result,
        Func<TOutcome, ValueTask<TValue>> success,
        Func<Error, ValueTask<TValue>> onError)
        => result.Error is null ? await success(result.OutCome!) : await onError(result.Error.Value);

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