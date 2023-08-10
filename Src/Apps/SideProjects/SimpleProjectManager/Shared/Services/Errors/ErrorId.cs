using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Akkatecture.Core;
using JetBrains.Annotations;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
[DataContract, MemoryPackable]
#pragma warning disable MA0097
public sealed partial class ErrorId : Identity<ErrorId>
    #pragma warning restore MA0097
{
    public ErrorId(string value) : base(value) { }

    [UsedImplicitly]
    public static bool TryParse(string? value, IFormatProvider? provider, [NotNullWhen(true)] out ErrorId? errorId)
        => TryParse(value, out errorId);
}