using System;
using JetBrains.Annotations;

namespace Tauron.Operations;

[PublicAPI]
public record SimpleResult<TOutCome>(TOutCome? OutCome, Error? Error);

[PublicAPI]
public record SimpleResult(Error? Error)
{
    public static SimpleResult Success() => new((Error?)null);

    public static SimpleResult<TResult> Success<TResult>(TResult result) => new(result, null);

    public static SimpleResult Failure(in Error error) => new(error);

    public static SimpleResult Failure(Exception error) => new(new Error(error));

    public static SimpleResult<TResult> Failure<TResult>(in Error error) => new(default, error);

    public static SimpleResult<TResult> Failure<TResult>(Exception error) => new(default, new Error(error));
}

[PublicAPI]
public static class SimpleResultExtensions
{
    public static TValue Match<TValue, TOutcome>(this SimpleResult<TOutcome> result, Func<TOutcome, TValue> success, Func<Error, TValue> onError)
        => result.Error is null ? success(result.OutCome!) : onError(result.Error.Value);
}