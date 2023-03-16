using System.Runtime.CompilerServices;

namespace Tauron.Operations;

public static class OperationResultExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowOnFail(this ref Option<Error> opt)
        => opt.OnSuccess(err => throw err.CreateException());


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ErrorToString(this ref Option<Error> opt)
        => opt.HasValue ? opt.Value.Info ?? opt.Value.Code : string.Empty;
}