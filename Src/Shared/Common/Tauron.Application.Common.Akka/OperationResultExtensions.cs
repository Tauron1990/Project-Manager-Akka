using System.Runtime.CompilerServices;
using Akka.Util;
using JetBrains.Annotations;
using Tauron.Operations;

namespace Tauron;

[PublicAPI]
public static class OperationResultExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ErrorToString<TData>(this Either<TData, Error> either)
        => either.Fold(_ => string.Empty, err => err.Info ?? err.Code);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TRes GetOrThrow<TRes>(this Either<TRes, Error> either)
        => either.Fold(r => r, err => throw err.CreateException());
}