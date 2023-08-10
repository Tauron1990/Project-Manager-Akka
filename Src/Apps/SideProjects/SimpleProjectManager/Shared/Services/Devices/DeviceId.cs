using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Akkatecture.Core;
using JetBrains.Annotations;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services.Devices;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
[DataContract, MemoryPackable]
public sealed partial class DeviceId : Identity<DeviceId>
{
    private static readonly Guid Namespace = new("14958213-06AB-4ACE-A6CF-897F67755015");

    public DeviceId(string value) : base(value) { }

    [UsedImplicitly]
    public static bool TryParse(string? value, IFormatProvider? provider, [NotNullWhen(true)] out DeviceId? deviceId)
        => TryParse(value, out deviceId);

    public static DeviceId ForName(string name)
        => NewDeterministic(Namespace, name);
}