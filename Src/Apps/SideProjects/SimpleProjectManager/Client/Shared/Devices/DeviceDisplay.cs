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
    
    private IEnumerable<ImmutableList<TData>> GroupToThree<TData>(IEnumerable<TData> enumerable)
    {
        var list = ImmutableList<TData>.Empty;

        foreach (TData data in enumerable)
        {
            list = list.Add(data);

            if(list.Count != 3)
                continue;

            yield return list;
            list = ImmutableList<TData>.Empty;
        }
        
        yield return list;
    }
}