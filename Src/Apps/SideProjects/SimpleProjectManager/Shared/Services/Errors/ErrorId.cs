using System.Diagnostics.CodeAnalysis;
using Akkatecture.Core;
using JetBrains.Annotations;

namespace SimpleProjectManager.Shared.Services;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
public sealed class ErrorId : Identity<ErrorId>
{
    public ErrorId(string value) : base(value) { }

    [UsedImplicitly]
    public static bool TryParse(string? value, IFormatProvider? provider, [NotNullWhen(true)] out ErrorId? errorId)
        => TryParse(value, out errorId);
}