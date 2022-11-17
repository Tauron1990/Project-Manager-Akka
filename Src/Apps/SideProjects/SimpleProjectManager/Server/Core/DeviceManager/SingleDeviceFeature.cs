﻿using System.Collections.Immutable;
using System.Reactive.Linq;
using Akka.Actor;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Server.Core.DeviceManager.Events;
using SimpleProjectManager.Shared.Services.Devices;
using static SimpleProjectManager.Client.Operations.Shared.Devices.DeviceManagerMessages;

namespace SimpleProjectManager.Server.Core.DeviceManager;

public sealed partial class SingleDeviceFeature : ActorFeatureBase<SingleDeviceFeature.State>
{
    public static Props New(DeviceInformations info, DeviceEventHandler handler)
        => Feature.Props(
            Feature.Create(
                () => new SingleDeviceFeature(),
                _ => new State(
                    info,
                    handler,
                    ImmutableDictionary<DeviceId, ISensorBox>.Empty,
                    ImmutableDictionary<DeviceId, bool>.Empty)));

    public override void PreStart()
    {
        Self.Tell(new InitState());
        base.PreStart();
    }

    protected override void ConfigImpl()
    {
        Receive<InitState>(obs => obs.Select(InitDeviceInfo));
        Receive<UpdateSensor>(obs => obs.Select(UpdateSensor));
        Receive<QuerySensorValue>(obs => obs.ToUnit(FindSensorValue));
        Receive<QueryButtonState>(obs => obs.ToUnit(FindButtonState));
        Receive<QueryUi>(obs => obs.ToUnit(p => p.Sender.Tell(new UiResponse(p.State.Info.RootUi))));
        Receive<UpdateButtonState>(obs => obs.Select(UpdateButton));
        Receive<ButtonClick>(obs => obs.ToUnit(p => p.State.Info.DeviceManager.Forward(p.Event)));
        Receive<Terminated>(obs => obs.ToUnit(p => p.Context.Stop(p.Self)));

        if(!CurrentState.Info.HasLogs) return;

        IActorRef? loggerActor = Context.ActorOf(LoggerActor.Create(Context.System, CurrentState.Info.DeviceId), $"Logger--{Guid.NewGuid():N}");
        Receive<QueryLoggerBatch>(obs => obs.ToUnit(p => loggerActor.Forward(p.Event)));
    }

    private State UpdateButton(StatePair<UpdateButtonState, State> arg)
    {
        (UpdateButtonState evt, State state) = arg;

        if(!state.ButtonStates.ContainsKey(evt.Identifer))
        {
            IdNotFound(Logger, "Button", evt.DeviceName, evt.Identifer);

            return state;
        }

        state.Handler.Publish(new ButtonStateUpdate(evt.DeviceName, evt.Identifer));

        return state with { ButtonStates = state.ButtonStates.SetItem(evt.Identifer, evt.State) };
    }

    private void FindButtonState(StatePair<QueryButtonState, State> obj)
    {
        (QueryButtonState evt, State state) = obj;

        if(state.ButtonStates.TryGetValue(evt.Identifer, out bool btnState))
        {
            obj.Sender.Tell(new ButtonStateResponse(btnState));

            return;
        }

        IdNotFound(Logger, "Button", evt.DeviceName, evt.Identifer);
        obj.Sender.Tell(new ButtonStateResponse(CanClick: false));
    }

    private void FindSensorValue(StatePair<QuerySensorValue, State> arg)
    {
        (QuerySensorValue evt, State state) = arg;

        if(!state.Sensors.TryGetValue(evt.Identifer, out ISensorBox? data))
        {
            IdNotFound(Logger, "Sensor", evt.DeviceName, evt.Identifer);
            arg.Sender.Tell(new SensorValueResult(SimpleResult.Failure("Id Not Found"), data));

            return;
        }

        arg.Sender.Tell(new SensorValueResult(SimpleResult.Success(), data));
    }

    [LoggerMessage(EventId = 62, Level = LogLevel.Warning, Message = "The type of the Sensor {id} from Device {device} dosn't match ({send}) with the Registration ({actual})")]
    private static partial void SensorTypeMismatch(ILogger logger, DeviceId id, DeviceId device, SensorType send, SensorType actual);

    [LoggerMessage(EventId = 63, Level = LogLevel.Warning, Message = "The Id {id} of Type {type} was not found for {device}")]
    private static partial void IdNotFound(ILogger logger, string type, DeviceId device, DeviceId id);

    private State UpdateSensor(StatePair<UpdateSensor, State> arg)
    {
        (UpdateSensor evt, State state) = arg;

        if(!state.Sensors.TryGetValue(evt.Identifer, out ISensorBox? data))
        {
            IdNotFound(Logger, "Sensor", evt.DeviceName, evt.Identifer);

            return state;
        }

        if(data.SensorType != evt.SensorValue.SensorType)
        {
            SensorTypeMismatch(Logger, evt.Identifer, evt.DeviceName, evt.SensorValue.SensorType, data.SensorType);

            return state;
        }

        state.Handler.Publish(new SensorUpdateEvent(evt.DeviceName, evt.Identifer, evt.SensorValue.SensorType));

        return state with { Sensors = state.Sensors.SetItem(evt.Identifer, evt.SensorValue) };
    }

    [LoggerMessage(EventId = 61, Level = LogLevel.Debug, Message = "Initialization of Device Interface with Name: {device}")]
    private static partial void InitSingleDevice(ILogger logger, DeviceId device);

    private State InitDeviceInfo(StatePair<InitState, State> p)
    {
        InitSingleDevice(Logger, p.State.Info.DeviceId);

        p.Context.Watch(p.State.Info.DeviceManager);

        var sensors = p.State.Sensors.AddRange(
            p.State.Info.CollectSensors()
               .Select(s => KeyValuePair.Create(s.Identifer, SensorBox.CreateDefault(s.SensorType))));

        var buttons = p.State.ButtonStates.AddRange(
            p.State.Info.CollectButtons()
               .Select(btn => KeyValuePair.Create(btn.Identifer, value: false)));

        return p.State with { Sensors = sensors, ButtonStates = buttons };
    }

    public sealed record InitState;

    public sealed record State(
        DeviceInformations Info, DeviceEventHandler Handler,
        ImmutableDictionary<DeviceId, ISensorBox> Sensors, ImmutableDictionary<DeviceId, bool> ButtonStates);
}