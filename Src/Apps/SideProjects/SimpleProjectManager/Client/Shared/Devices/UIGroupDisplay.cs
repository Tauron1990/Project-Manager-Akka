using System.Collections.Immutable;
using Microsoft.AspNetCore.Components;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Client.Shared.Devices;

public partial class UIGroupDisplay
{
    [Parameter]
    public DeviceUiGroup? UiGroup { get; set; }

    [Parameter]
    public bool ShowCategory { get; set; } = true;

    [Parameter]
    public DeviceId? DeviceId { get; set; }

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