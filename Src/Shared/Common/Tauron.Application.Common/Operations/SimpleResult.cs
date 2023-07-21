using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Tauron.Operations;

[PublicAPI]
[StructLayout(LayoutKind.Auto)]
public readonly record struct SimpleResult<TOutCome>(TOutCome? OutCome, Error? Error);

[PublicAPI]
public readonly record struct SimpleResult(Error? Error)
{
    public static SimpleResult FromOperation(IOperationResult operationResult)
        => operationResult.Ok ? Success() : Failure(operationResult.Errors?.FirstOrDefault() ?? new Error("Unbekannt", "Unkowen"));

    public static SimpleResult Success() => new(null);

    public static SimpleResult<TResult> Success<TResult>(TResult result) => new(result, null);

    public static SimpleResult Failure(in Error error) => new(error);

    public static SimpleResult Failure(string error) => new(new Error(null, error));

    public static SimpleResult Failure(Exception error, IFormatProvider? provider = null) => new(Operations.Error.FromException(error, provider));

    public static SimpleResult<TResult> Failure<TResult>(in Error error) => new(default, error);

    public static SimpleResult<TResult> Failure<TResult>(Exception error, IFormatProvider? provider = null) => new(default, Operations.Error.FromException(error, provider));

    public static async ValueTask<SimpleResult> Catch(Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
            return Success();
        }
        catch (Exception e)
        {
            return Failure(e);
        }
    }
    
    public static async ValueTask<SimpleResult> Catch(ValueTask task)
    {
        try
        {
            await task.ConfigureAwait(false);
            return Success();
        }
        catch (Exception e)
        {
            return Failure(e);
        }
    }
}