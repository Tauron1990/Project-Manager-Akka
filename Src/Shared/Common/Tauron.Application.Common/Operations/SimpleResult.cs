using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Tauron.Operations;

[PublicAPI, StructLayout(LayoutKind.Auto)]
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