using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Channels;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Operation.Client.Config;
using SimpleProjectManager.Operation.Client.Device.MGI.Logging;
using SimpleProjectManager.Operation.Client.Device.MGI.MgiUi;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion;
using Tauron.Application;
using Tauron.Operations;
using Tauron.TAkka;

namespace SimpleProjectManager.Operation.Client.Device.MGI;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
public sealed class MgiMachine : IMachine
{
    private const string MgiId = "CFDD2F56-AD5C-4A7C-A3B5-535A46B21EC5";
    private const int Port = 23421;
    
    private readonly LogCollector<LogInfo> _logCollector;
    private readonly DeviceId _deviceId;
    private readonly ObjectName _clientName;

    private readonly MgiUiManager _uiManager;

#pragma warning disable GU0073
    public MgiMachine(
        ILoggerFactory loggerFactory, 
        OperationConfiguration operationConfiguration, 
        ITauronEnviroment tauronEnviroment,
        IStateFactory stateFactory)
#pragma warning restore GU0073
    {
        _logCollector = new LogCollector<LogInfo>(
            "MGI_Client",
            loggerFactory.CreateLogger<LogCollector<LogInfo>>(),
            ConvertLogInfo);
        
        _deviceId = operationConfiguration.CreateDeviceId(MgiId);
        _clientName = operationConfiguration.Name;

        _uiManager = new MgiUiManager(loggerFactory.CreateLogger<MgiUiManager>(), tauronEnviroment, stateFactory);
    }

    private LogData ConvertLogInfo(LogInfo element)
    {
        _uiManager.ProcessStatus(element);
        
        return new LogData(
            string.Equals(element.Type.ToLower(CultureInfo.InvariantCulture), "error", StringComparison.Ordinal)
                ? LogLevel.Error
                : LogLevel.Trace,
            GetCategory(element.Type),
            SimpleMessage.From($"{element.Type}; {element.Content}"),
            element.TimeStamp,
            ImmutableDictionary<string, PropertyValue>.Empty.Add("Application", PropertyValue.From(element.Application)));

        LogCategory GetCategory(string type) =>
            type.ToLower(CultureInfo.InvariantCulture) switch
            {
                "cudalib" => LogCategory.From("Scanner"),
                "scanner" => LogCategory.From("Scanner"),
                "gis" => LogCategory.From("Scanner"),
                _ => LogCategory.From("Maschiene"),
            };
    }

    public Task Init(IActorContext context)
    {
        var channel = Channel.CreateBounded<LogInfo>(
            new BoundedChannelOptions(3000)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
            });
        
        _logCollector.CollectLogs(channel.Reader);

        context.ActorOf(() => new LoggerServer(channel.Writer, Port), "Logging_Server");
        
        return Task.CompletedTask;
    }

    public Task<DeviceInformations> CollectInfo()
        => Task.FromResult(new DeviceInformations(
            _deviceId, 
            DeviceName.From($"{_clientName} MGI"), 
            HasLogs:true, 
            _uiManager.CreateUi(),
            ImmutableList<ButtonState>.Empty,
            Nobody.Instance));

    public IState<DeviceUiGroup>? UIUpdates()
        => _uiManager.DynamicUi;

    public Task<DeviceManagerMessages.ISensorBox> UpdateSensorValue(DeviceSensor sensor)
        => _uiManager.UpdateSensorValue(sensor);

    public void ButtonClick(DeviceId identifer)
        => _uiManager.ButtonClick(identifer);

    public void WhenButtonStateChanged(DeviceId identifer, Action<bool> onButtonStateChanged)
        => _uiManager.WhenButtonStateChanged(identifer, onButtonStateChanged);

    public Task<SimpleResult> NewInput(DeviceId element, string input) => _uiManager.NewInput(element, input);

    public Task<LogBatch> NextLogBatch()
        => _logCollector.GetLogs(_deviceId);
    
}