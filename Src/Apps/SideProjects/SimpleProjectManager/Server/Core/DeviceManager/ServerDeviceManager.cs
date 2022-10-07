using System.Collections.Immutable;
using System.Reactive.Linq;
using Akka.Actor;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Server.Core.DeviceManager.Events;
using static SimpleProjectManager.Client.Operations.Shared.Devices.DeviceManagerMessages;

namespace SimpleProjectManager.Server.Core.DeviceManager;


public sealed partial class ServerDeviceManagerFeature : ActorFeatureBase<ServerDeviceManagerFeature.State>
{
    public static Props Create(DeviceEventHandler deviceEvents)
        => Feature.Props(Feature.Create(() => new ServerDeviceManagerFeature(), _ => new State(ImmutableDictionary<string, DeviceInformations>.Empty, deviceEvents)));

    public sealed record State(ImmutableDictionary<string, DeviceInformations> RegisteredDevices, DeviceEventHandler Events);

    protected override void ConfigImpl()
    {
        Receive<DeviceInformations>(HandleNewDevice);
    }

    [LoggerMessage(EventId = 57, Level = LogLevel.Debug, Message = "New Device Registration Incomming from {path} with {name}")]
    private static partial void NewDeviceRegistration(ILogger logger, string name, ActorPath path);
    
    private IObservable<State> HandleNewDevice(IObservable<StatePair<DeviceInformations, State>> obs)
        => obs.Select(
            pair =>
            {
                var (evt, state) = pair;
                NewDeviceRegistration(Logger, evt.DeviceName, pair.Sender.Path);
                
                if(state.RegisteredDevices.ContainsKey(evt.DeviceName))
                {
                    pair.Sender.Tell(new DeviceInfoResponse("Duplicate Device Registration"));
                    return state;
                }

                state.Events.Publish(new NewDeviceEvent(evt));
                pair.Sender.Tell(new DeviceInfoResponse(null));
                return state with { RegisteredDevices = state.RegisteredDevices.Add(evt.DeviceName, evt) };
            });
}