using System.Reactive.Linq;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.ViewModels.Devices;
using Tauron;

namespace SimpleProjectManager.Client.Shared.Devices;

public sealed partial class DevicesDisplay
{
    private DevicePair? _selectedPair;

    private DevicePair? SelectedPair
    {
        get => _selectedPair;
        set => this.RaiseAndSetIfChanged(ref _selectedPair, value);
    }

    protected override IEnumerable<IDisposable> InitializeModel()
    {
        yield return this.Bind(ViewModel, m => m.SelectedPair, v => v.SelectedPair);
    }
}