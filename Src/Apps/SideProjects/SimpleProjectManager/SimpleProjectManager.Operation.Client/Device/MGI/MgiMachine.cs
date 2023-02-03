using System.Collections.Immutable;
using System.Threading.Channels;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Operation.Client.Config;
using SimpleProjectManager.Operation.Client.Device.MGI.Logging;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion;
using Tauron.TAkka;

namespace SimpleProjectManager.Operation.Client.Device.MGI;

internal sealed class MgiMachine : IMachine
{
    private const string MgiId = "CFDD2F56-AD5C-4A7C-A3B5-535A46B21EC5";
    private const int Port = 23421;
    
    private readonly LogCollector<LogInfo> _logCollector;
    private readonly DeviceId _deviceId;
    private readonly ObjectName _clientName;

    public MgiMachine(ILoggerFactory loggerFactory, OperationConfiguration operationConfiguration)
    {
        _logCollector = new LogCollector<LogInfo>(
            "MGI_Client",
            loggerFactory.CreateLogger<LogCollector<LogInfo>>(),
            element =>
                new LogData(
                    LogLevel.Trace,
                    LogCategory.From(element.Type),
                    SimpleMessage.From(element.Content),
                    element.TimeStamp,
                    ImmutableDictionary<string, PropertyValue>.Empty
                       .Add("Application", PropertyValue.From(element.Application))));
        
        _deviceId = operationConfiguration.CreateDeviceId(MgiId);
        _clientName = operationConfiguration.Name;
    }

    public Task Init(IActorContext context)
    {
        var channel = Channel.CreateBounded<LogInfo>(
            new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
            });
        
        _logCollector.CollectLogs(channel.Reader);
        context.ActorOf(() => new LoggerServer(channel.Writer, Port), "LoggingServer");
        
        return Task.CompletedTask;
    }

    public Task<DeviceInformations> CollectInfo()
        => Task.FromResult(new DeviceInformations(
            _deviceId, 
            DeviceName.From($"{_clientName} MGI"), 
            HasLogs:true, 
            DeviceUi.Text("Keine UI"),
            ImmutableList<ButtonState>.Empty,
            Nobody.Instance));

    public IState<DeviceUiGroup>? UIUpdates()
        => null;

    public Task<DeviceManagerMessages.ISensorBox> UpdateSensorValue(DeviceSensor sensor)
        => Task.FromResult(DeviceManagerMessages.SensorBox.CreateDefault(sensor.SensorType));

    public void ButtonClick(DeviceId identifer)
    {
        
    }

    public void WhenButtonStateChanged(DeviceId identifer, Action<bool> onButtonStateChanged)
    {
        
    }

    public Task<LogBatch> NextLogBatch()
        => _logCollector.GetLogs(_deviceId);
    
}