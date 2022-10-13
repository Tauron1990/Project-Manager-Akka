using System.Reactive;
using Akka.Actor;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Server.Core.DeviceManager.Events;
using static SimpleProjectManager.Client.Operations.Shared.Devices.DeviceManagerMessages;

namespace SimpleProjectManager.Server.Core.DeviceManager;


public sealed partial class ServerDeviceManagerFeature : ActorFeatureBase<ServerDeviceManagerFeature.State>
{
    public static Props Create(DeviceEventHandler deviceEvents)
        => Feature.Props(Feature.Create(() => new ServerDeviceManagerFeature(), _ => new State(deviceEvents)));

    public sealed record State(DeviceEventHandler Events);

    protected override void ConfigImpl()
    {
        Receive<DeviceInformations>(HandleNewDevice);
        Receive<QueryDevices>(obs => obs.ToUnit(p => p.Sender.Tell(new DevicesResponse(p.Context.GetChildren().Select(r => r.Path.Name).ToArray()))));
        
        Receive<IDeviceCommand>(obs => obs.ToUnit(p => p.Context.Child(p.Event.DeviceName).Forward(p.Event)));
        Receive<Terminated>(obs => obs.ToUnit(p => p.State.Events.Publish(new DeviceRemoved(p.Event.ActorRef.Path.Name))));
    }

    [LoggerMessage(EventId = 57, Level = LogLevel.Debug, Message = "New Device Registration Incomming from {path} with {name}")]
    private static partial void NewDeviceRegistration(ILogger logger, string name, ActorPath path);

    [LoggerMessage(EventId = 58, Level = LogLevel.Warning, Message = "Duplicate Device {name} Registration From {path}")]
    private static partial void DuplicateDeviceRegistration(ILogger logger, string name, ActorPath path);

    [LoggerMessage(EventId = 59, Level = LogLevel.Warning, Message = "Empty DeviceName form {path}")]
    private static partial void EmptyDeviceNameRegistration(ILogger logger, ActorPath path);

    [LoggerMessage(EventId = 60, Level = LogLevel.Error, Message = "Invalid ActorName from {path}")]
    private static partial void InvalidActorName(ILogger logger, Exception ex, ActorPath path);
    
    private IObservable<Unit> HandleNewDevice(IObservable<StatePair<DeviceInformations, State>> obs)
        => obs.ToUnit(
            pair =>
            {
                var (evt, state) = pair;
                NewDeviceRegistration(Logger, evt.DeviceName, pair.Sender.Path);

                if(string.IsNullOrWhiteSpace(evt.DeviceName))
                {
                    EmptyDeviceNameRegistration(Logger, pair.Sender.Path);
                    pair.Sender.Tell(new DeviceInfoResponse(false, "Empty Device Name"));
                    return;
                }
                    
                
                if(!pair.Context.Child(evt.DeviceName).Equals(ActorRefs.Nobody))
                {
                    DuplicateDeviceRegistration(Logger, evt.DeviceName, pair.Sender.Path);
                    pair.Sender.Tell(new DeviceInfoResponse(true, "Duplicate Device Registration"));
                    return;
                }

                try
                {
                    pair.Context.Watch(pair.Context.ActorOf(SingleDeviceFeature.New(evt, state.Events), evt.DeviceName));
                    state.Events.Publish(new NewDeviceEvent(evt));
                    pair.Sender.Tell(new DeviceInfoResponse(false, null));
                }
                catch (InvalidActorNameException exception)
                {
                    InvalidActorName(Logger, exception, pair.Sender.Path);
                    pair.Sender.Tell(new DeviceInfoResponse(false, exception.Message));
                }
            });
}