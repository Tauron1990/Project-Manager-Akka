using SimpleProjectManager.Client.Shared.Devices;
using SimpleProjectManager.Client.Shared.ViewModels.Devices;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion;
using Tauron.Application;
using Tauron.Application.Blazor;
using Tauron.Application.Blazor.Parameters;

namespace SimpleProjectManager.Client.ViewModels.Devices;

public sealed class LogsDisplayViewModel : LogViewModel, IParameterUpdateable
{
    private readonly IEventAggregator _eventAggregator;

    public LogsDisplayViewModel(IStateFactory stateFactory, IDeviceService deviceService, IEventAggregator eventAggregator) 
        : base(stateFactory, deviceService)
    {
        _eventAggregator = eventAggregator;
    }

    protected override void ErrorOnFetchCurrentLogs(Exception exception)
        => _eventAggregator.PublishError(exception);

    protected override IState<DeviceId?> GetDevice(IStateFactory stateFactory)
        => Updater.Register<DeviceId?>(nameof(LogsDisplay.DeviceId), stateFactory);

    public ParameterUpdater Updater { get; } = new();
}