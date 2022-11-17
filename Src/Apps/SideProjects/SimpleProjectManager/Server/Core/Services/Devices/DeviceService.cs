using System.Collections.Immutable;
using Akka.Actor;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Server.Core.DeviceManager.Events;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion;
using static SimpleProjectManager.Client.Operations.Shared.Devices.DeviceManagerMessages;

namespace SimpleProjectManager.Server.Core.Services.Devices;

public partial class DeviceService : IDeviceService, IDisposable
{
    private readonly ActorSelection _deviceManagerSelection;
    private readonly ILogger<DeviceService> _logger;
    private readonly IDisposable _subscription;

    private IActorRef? _deviceManager;
    private DateTime _lastLog = DateTime.MinValue;

    public DeviceService(ActorSystem actorSystem, DeviceEventHandler handler, ILogger<DeviceService> logger)
    {
        _logger = logger;
        _deviceManagerSelection = actorSystem.ActorSelection(DeviceInformations.ManagerPath);

        _subscription = handler.Get().Subscribe(ProcessEvent);
    }

    public virtual Task<DateTime> CurrentLogs(CancellationToken token)
        => token.IsCancellationRequested ? Task.FromCanceled<DateTime>(token) : Task.FromResult(_lastLog);

    public virtual void Dispose()
        => _subscription.Dispose();

    // ReSharper disable once CognitiveComplexity
    private void ProcessEvent(IDeviceEvent evt)
    {
        switch (evt)
        {
            case ButtonStateUpdate buttonStateUpdate:
                using (Computed.Invalidate())
                {
                    _ = CanClickButton(buttonStateUpdate.Device, buttonStateUpdate.Identifer, CancellationToken.None);
                }

                break;
            case NewBatchesArrived newBatchesArrived:
                _lastLog = newBatchesArrived.Date;
                using (Computed.Invalidate())
                {
                    _ = CurrentLogs(CancellationToken.None);
                }

                break;
            case NewDeviceEvent newDeviceEvent:
                using (Computed.Invalidate())
                {
                    _ = GetAllDevices(CancellationToken.None);
                    _ = GetRootUi(newDeviceEvent.Device, CancellationToken.None);
                }

                break;
            case SensorUpdateEvent sensorUpdateEvent:
                using (Computed.Invalidate())
                {
                    switch (sensorUpdateEvent.SensorType)
                    {
                        case SensorType.Double:
                            _ = GetDoubleSensorValue(sensorUpdateEvent.Device, sensorUpdateEvent.Identifer, CancellationToken.None);

                            break;
                        case SensorType.String:
                            _ = GetStringSensorValue(sensorUpdateEvent.Device, sensorUpdateEvent.Identifer, CancellationToken.None);

                            break;
                        case SensorType.Number:
                            _ = GetIntSensorValue(sensorUpdateEvent.Device, sensorUpdateEvent.Identifer, CancellationToken.None);

                            break;
                    }
                }

                break;
            case DeviceRemoved:
                _ = GetAllDevices(CancellationToken.None);

                break;
        }
    }

    private async Task<IActorRef> GetDeviceManager()
    {
        _deviceManager ??= await _deviceManagerSelection.ResolveOne(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        return _deviceManager ?? ActorRefs.Nobody;
    }

    [LoggerMessage(EventId = 65, Level = LogLevel.Error, Message = "Error on Process Ask DeviceManager")]
    private partial void ErrorOnProcessRequest(Exception ex);

    private async Task<TResult> Run<TResult>(Func<IActorRef, Task<TResult>> runner)
    {
        try
        {
            return await runner(await GetDeviceManager().ConfigureAwait(false)).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            ErrorOnProcessRequest(e);

            throw;
        }
    }

    public virtual async Task<DeviceList> GetAllDevices(CancellationToken token)
        => await Run(
            async man =>
            {
                var result = await man.Ask<DevicesResponse>(new QueryDevices(), TimeSpan.FromSeconds(10), token).ConfigureAwait(false);

                return new DeviceList(result.Devices.Select(d => new FoundDevice(d.Value, d.Key)).ToImmutableArray());
            }).ConfigureAwait(false);

    public virtual async Task<DeviceUiGroup> GetRootUi(DeviceId device, CancellationToken token)
        => await Run(
            async man =>
            {
                var result = await man.Ask<UiResponse>(new QueryUi(device), TimeSpan.FromSeconds(10), token).ConfigureAwait(false);

                return result.Root;
            }).ConfigureAwait(false);

    public virtual async Task<string> GetStringSensorValue(DeviceId device, DeviceId sensor, CancellationToken token)
        => await Run(
            async man =>
            {
                var valueResult = await man.Ask<SensorValueResult>(new QuerySensorValue(device, sensor), TimeSpan.FromSeconds(10), token).ConfigureAwait(false);

                if(valueResult.Result.IsSuccess())
                    return valueResult.Value?.AsString ?? string.Empty;

                throw new InvalidOperationException(valueResult.Result.GetErrorString());
            }).ConfigureAwait(false);

    public virtual async Task<int> GetIntSensorValue(DeviceId device, DeviceId sensor, CancellationToken token)
        => await Run(
            async man =>
            {
                var valueResult = await man.Ask<SensorValueResult>(new QuerySensorValue(device, sensor), TimeSpan.FromSeconds(10), token).ConfigureAwait(false);

                if(valueResult.Result.IsSuccess())
                    return valueResult.Value?.AsInt ?? -1;

                throw new InvalidOperationException(valueResult.Result.GetErrorString());
            }).ConfigureAwait(false);

    public virtual async Task<double> GetDoubleSensorValue(DeviceId device, DeviceId sensor, CancellationToken token)
        => await Run(
            async man =>
            {
                var valueResult = await man.Ask<SensorValueResult>(new QuerySensorValue(device, sensor), TimeSpan.FromSeconds(10), token).ConfigureAwait(false);

                if(valueResult.Result.IsSuccess())
                    return valueResult.Value?.AsDouble ?? -1d;

                throw new InvalidOperationException(valueResult.Result.GetErrorString());
            }).ConfigureAwait(false);

    public virtual async Task<bool> CanClickButton(DeviceId device, DeviceId button, CancellationToken token)
        => await Run(
            async man =>
            {
                var result = await man.Ask<ButtonStateResponse>(new QueryButtonState(device, button), TimeSpan.FromSeconds(10), token).ConfigureAwait(false);

                return result.CanClick;
            }).ConfigureAwait(false);

    public async Task<Logs> GetBatches(DeviceId deviceName, DateTime from, CancellationToken token)
        => await Run(
            async man =>
            {
                var result = await man.Ask<LoggerBatchResult>(new QueryLoggerBatch(deviceName, from), TimeSpan.FromSeconds(10), token).ConfigureAwait(false);

                return new Logs(result.Batches.ToImmutableList());
            }).ConfigureAwait(false);

    public async Task<SimpleResult> ClickButton(DeviceId device, DeviceId button, CancellationToken token)
        => await Run(
            man =>
            {
                man.Tell(new ButtonClick(device, button));

                return Task.FromResult(SimpleResult.Success());
            }).ConfigureAwait(false);
}