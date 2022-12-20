using System.Diagnostics.CodeAnalysis;
using Akkatecture.Core;
using JetBrains.Annotations;

namespace SimpleProjectManager.Shared.Services.Devices;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
public sealed class DeviceId : Identity<DeviceId>
{
    private static readonly Guid _namespace = new Guid("14958213-06AB-4ACE-A6CF-897F67755015");

    public DeviceId(string value) : base(value) { }

    [UsedImplicitly]
    public static bool TryParse(string? value, IFormatProvider? provider, [NotNullWhen(true)] out DeviceId? deviceId)
        => TryParse(value, out deviceId);

    public static DeviceId ForName(string name)
        => NewDeterministic(_namespace, name);
}