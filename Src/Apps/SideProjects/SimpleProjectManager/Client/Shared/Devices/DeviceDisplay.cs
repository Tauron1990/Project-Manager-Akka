using System.Collections.Immutable;
using Microsoft.AspNetCore.Components;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.ViewModels.Devices;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion;

namespace SimpleProjectManager.Client.Shared.Devices;

public sealed partial class DeviceDisplay
{

    [Parameter]
    public DevicePair? ToDisplay { get; set; }
    
}