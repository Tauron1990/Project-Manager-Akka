using System.Diagnostics.CodeAnalysis;
using Akkatecture.Core;
using JetBrains.Annotations;

namespace SimpleProjectManager.Shared.Services;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
#pragma warning disable MA0097
public sealed class ErrorId : Identity<ErrorId>
    #pragma warning restore MA0097
{
    public ErrorId(string value) : base(value) { }

    [UsedImplicitly]
    public static bool TryParse(string? value, IFormatProvider? provider, [NotNullWhen(true)] out ErrorId? errorId)
        => TryParse(value, out errorId);
}