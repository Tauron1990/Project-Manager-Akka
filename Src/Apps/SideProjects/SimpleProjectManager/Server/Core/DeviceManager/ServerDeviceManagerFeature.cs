using System.Collections.Immutable;
using System.Reactive.Linq;
using Akka.Actor;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Server.Core.DeviceManager.Events;
using SimpleProjectManager.Shared.Services.Devices;
using static SimpleProjectManager.Client.Operations.Shared.Devices.DeviceManagerMessages;

namespace SimpleProjectManager.Server.Core.DeviceManager;

public sealed partial class
    ServerDeviceManagerFeature : ActorFeatureBase<ServerDeviceManagerFeature.State>
{
    public static Props Create(DeviceEventHandler deviceEvents)
        => Feature.Props(Feature.Create(() => new ServerDeviceManagerFeature(), _ => new State(deviceEvents, ImmutableDictionary<DeviceId, DeviceName>.Empty)));

    protected override void ConfigImpl()
    {
        Context.ActorOf<DeviceClusterManager>();
        
        Receive<DeviceChanged>(HandleNewDevice);
        Receive<QueryDevices>(obs => obs.ToUnit(p => p.Sender.Tell(new DevicesResponse(p.State.Devices))));

        Receive<IDeviceCommand>(obs => obs.ToUnit(p => p.Context.Child(p.Event.DeviceName.Value).Forward(p.Event)));
        Receive<Terminated>(
            obs => obs.Select(
                p =>
                {
                    if(!p.State.Devices.ContainsKey(new DeviceId(p.Event.ActorRef.Path.Name)))
                        return p.State;
                    
                    p.State.Events.Publish(new DeviceRemoved(new DeviceId(p.Event.ActorRef.Path.Name)));

                    return p.State with { Devices = p.State.Devices.Remove(new DeviceId(p.Event.ActorRef.Path.Name)) };
                }));
    }

    [LoggerMessage(EventId = 57, Level = LogLevel.Debug, Message = "New Device Registration Incomming from {path} with {name} and {id}")]
    #pragma warning disable EPS05
    private static partial void NewDeviceRegistration(ILogger logger, DeviceId id, DeviceName name, ActorPath path);
    #pragma warning restore EPS05

    // [LoggerMessage(EventId = 58, Level = LogLevel.Warning, Message = "Duplicate Device {name} Registration From {path}")]
    // private static partial void DuplicateDeviceRegistration(ILogger logger, DeviceId name, ActorPath path);

    [LoggerMessage(EventId = 59, Level = LogLevel.Warning, Message = "Empty DeviceName form {path}")]
    private static partial void EmptyDeviceNameRegistration(ILogger logger, ActorPath path);

    [LoggerMessage(EventId = 60, Level = LogLevel.Error, Message = "Invalid ActorName from {path}")]
    private static partial void InvalidActorName(ILogger logger, Exception ex, ActorPath path);

    private IObservable<State> HandleNewDevice(IObservable<StatePair<DeviceChanged, State>> obs)
        => obs.Select(
            pair =>
            {
                ((DeviceChangedType change, DeviceInformations device), State state) = pair;
                NewDeviceRegistration(Logger, device.DeviceId, device.Name, pair.Sender.Path);

                if(string.IsNullOrWhiteSpace(device.Name.Value))
                {
                    EmptyDeviceNameRegistration(Logger, pair.Sender.Path);
                    //pair.Sender.Tell(new DeviceInfoResponse(Duplicate: false, Result: SimpleResult.Failure("Empty Device Name")));

                    return state;
                }

                switch (change)
                {
                    case DeviceChangedType.Add:
                        try
                        {
                            State newState = state with
                                             {
                                                 Devices = state.Devices.Add(device.DeviceId, device.Name),
                                             };

                            pair.Context.Watch(pair.Context.ActorOf(SingleDeviceFeature.New(device, state.Events), device.DeviceId.Value));
                            state.Events.Publish(new NewDeviceEvent(device));
                            //pair.Sender.Tell(new DeviceInfoResponse(Duplicate: false, Result: SimpleResult.Success()));

                            return newState;
                        }
                        catch (InvalidActorNameException exception)
                        {
                            InvalidActorName(Logger, exception, pair.Sender.Path);
                            //pair.Sender.Tell(new DeviceInfoResponse(Duplicate: false, Result: SimpleResult.Failure(exception.Message)));

                            return state;
                        }
                    case DeviceChangedType.Remove:
                        IActorRef? actor = Context.Child(device.DeviceId.Value);
                        if(actor is not null)
                        {
                            Context.Unwatch(actor);
                            Context.Stop(actor);
                            state.Events.Publish(new DeviceRemoved(device.DeviceId));

                            return state with { Devices = state.Devices.Remove(device.DeviceId) };
                        }
                        break;
                    case DeviceChangedType.Changed:
                        Context.Child(device.DeviceId.Value)?.Tell(device);
                        state.Events.Publish(new DeviceUpdated(device.DeviceId, device));
                        break;
                    default:
                        return state;
                }

                return state;
            });

    public sealed record State(DeviceEventHandler Events, ImmutableDictionary<DeviceId, DeviceName> Devices);
}