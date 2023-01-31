using Akka.Actor;
using Akka.DistributedData;
using JetBrains.Annotations;

namespace SimpleProjectManager.Client.Operations.Shared.Devices;

[PublicAPI]
public sealed class DeviceInfoEntry : IReplicatedData<DeviceInfoEntry>
{
    private readonly DateTime _dateTime = DateTime.UtcNow;

    public IActorRef Client { get; }

    public DeviceInformations DeviceInformations { get; }

    private DeviceInfoEntry(IActorRef client, DeviceInformations deviceInformations)
    {
        Client = client;
        DeviceInformations = deviceInformations;
    }

    public static DeviceInfoEntry Create(IActorRef actor, DeviceInformations info) => new(actor, info);

    public DeviceInfoEntry With(IActorRef actor) => new(actor, DeviceInformations);

    public DeviceInfoEntry With(DeviceInformations device) => new(Client, device);
    
    public DeviceInfoEntry Merge(DeviceInfoEntry other)
        => other._dateTime > _dateTime ? other : this;

    public IReplicatedData Merge(IReplicatedData other)
        => Merge((DeviceInfoEntry)other);
}