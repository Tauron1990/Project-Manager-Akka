using System.Diagnostics.CodeAnalysis;
using Akkatecture.Core;
using JetBrains.Annotations;

namespace SimpleProjectManager.Shared.Services.Devices;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
public sealed class DeviceId : Identity<DeviceId>
{
    public DeviceId(string value) : base(value) { }
    
    [UsedImplicitly]
    public static bool TryParse(string? value, IFormatProvider? provider, [NotNullWhen(true)] out DeviceId? deviceId)
        => Identity<DeviceId>.TryParse(value, out deviceId);
}