using System.Collections.Immutable;
using System.Reactive.Linq;
using Akka.Actor;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Server.Core.DeviceManager.Events;
using SimpleProjectManager.Shared.Services.Devices;
using static SimpleProjectManager.Client.Operations.Shared.Devices.DeviceManagerMessages;

namespace SimpleProjectManager.Server.Core.DeviceManager;

public sealed partial class ServerDeviceManagerFeature : ActorFeatureBase<ServerDeviceManagerFeature.State>
{
    public static Props Create(DeviceEventHandler deviceEvents)
        => Feature.Props(Feature.Create(() => new ServerDeviceManagerFeature(), _ => new State(deviceEvents, ImmutableDictionary<DeviceId, DeviceName>.Empty)));

    protected override void ConfigImpl()
    {
        Receive<DeviceInformations>(HandleNewDevice);
        Receive<QueryDevices>(obs => obs.ToUnit(p => p.Sender.Tell(new DevicesResponse(p.State.Devices))));

        Receive<IDeviceCommand>(obs => obs.ToUnit(p => p.Context.Child(p.Event.DeviceName.Value).Forward(p.Event)));
        Receive<Terminated>(obs => obs.ToUnit(p => p.State.Events.Publish(new DeviceRemoved(new DeviceId(p.Event.ActorRef.Path.Name)))));
    }

    [LoggerMessage(EventId = 57, Level = LogLevel.Debug, Message = "New Device Registration Incomming from {path} with {name} and {id}")]
    private static partial void NewDeviceRegistration(ILogger logger, DeviceId id, DeviceName name, ActorPath path);

    [LoggerMessage(EventId = 58, Level = LogLevel.Warning, Message = "Duplicate Device {name} Registration From {path}")]
    private static partial void DuplicateDeviceRegistration(ILogger logger, DeviceId name, ActorPath path);

    [LoggerMessage(EventId = 59, Level = LogLevel.Warning, Message = "Empty DeviceName form {path}")]
    private static partial void EmptyDeviceNameRegistration(ILogger logger, ActorPath path);

    [LoggerMessage(EventId = 60, Level = LogLevel.Error, Message = "Invalid ActorName from {path}")]
    private static partial void InvalidActorName(ILogger logger, Exception ex, ActorPath path);

    private IObservable<State> HandleNewDevice(IObservable<StatePair<DeviceInformations, State>> obs)
        => obs.Select(
            pair =>
            {
                (DeviceInformations evt, State state) = pair;
                NewDeviceRegistration(Logger, evt.DeviceId, evt.Name, pair.Sender.Path);

                if(string.IsNullOrWhiteSpace(evt.Name.Value))
                {
                    EmptyDeviceNameRegistration(Logger, pair.Sender.Path);
                    pair.Sender.Tell(new DeviceInfoResponse(Duplicate: false, Result: SimpleResult.Failure("Empty Device Name")));

                    return state;
                }


                if(!state.Devices.ContainsKey(evt.DeviceId))
                {
                    DuplicateDeviceRegistration(Logger, evt.DeviceId, pair.Sender.Path);
                    pair.Sender.Tell(new DeviceInfoResponse(Duplicate: true, SimpleResult.Failure("Duplicate Device Registration")));

                    return state;
                }

                try
                {
                    var newState = state with
                                   {
                                       Devices = state.Devices.Add(evt.DeviceId, evt.Name)
                                   };
                    
                    pair.Context.Watch(pair.Context.ActorOf(SingleDeviceFeature.New(evt, state.Events), evt.DeviceId.Value));
                    state.Events.Publish(new NewDeviceEvent(evt));
                    pair.Sender.Tell(new DeviceInfoResponse(Duplicate: false, Result: SimpleResult.Success()));

                    return newState;
                }
                catch (InvalidActorNameException exception)
                {
                    InvalidActorName(Logger, exception, pair.Sender.Path);
                    pair.Sender.Tell(new DeviceInfoResponse(Duplicate: false, SimpleResult.Failure(exception.Message)));

                    return state;
                }
            });

    public sealed record State(DeviceEventHandler Events, ImmutableDictionary<DeviceId, DeviceName> Devices);
}