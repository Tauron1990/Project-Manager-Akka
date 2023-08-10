using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Devices;
using SimpleProjectManager.Client.Shared.Services;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion;
using Tauron;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels.Devices;

public class DeviceInputViewModel : BlazorViewModel
{
    private string _input = string.Empty;

    public string Input
    {
        get => _input;
        set => this.RaiseAndSetIfChanged(ref _input, value);
    }

    public ReactiveCommand<Unit, Unit> Send { get; }

    protected DeviceInputViewModel(IStateFactory stateFactory, IMessageDispatcher aggregator, IDeviceService service) : base(stateFactory)
    {
        var deviceId = GetParameter<DeviceId>(nameof(DeviceInputDisplay.DeviceId));
        var element = GetParameter<DeviceId>(nameof(DeviceInputDisplay.Element));

        (
                from text in GetParameter<string>(nameof(DeviceInputDisplay))
                    .ToObservable(aggregator.PublishError)
                where !string.IsNullOrWhiteSpace(text)
                select text
            )
            .DistinctUntilChanged()
            .Subscribe(s => Input = s)
            .DisposeWith(this);

        var canClick = new Subject<bool>().DisposeWith(this);

        Send = ReactiveCommand.CreateFromTask(SendExecute, canClick.StartWith(true));
        Send.IsExecuting.Select(b => !b).Subscribe(canClick).DisposeWith(this);

        async Task SendExecute()
        {
            DeviceId? device = deviceId.ValueOrDefault;
            DeviceId? elementId = element.ValueOrDefault;
            if(elementId is null || device is null)
                return;

            try
            {
                await aggregator.IsSuccess(
                    () => TimeoutToken.WithDefault(
                        default,
                        async t => await service.DeviceInput(new DeviceInputData(device, elementId, Input), t).ConfigureAwait(false)
                    )).ConfigureAwait(false);

            }
            catch (Exception e)
            {
                aggregator.PublishError(e);
            }
        }
    }
}