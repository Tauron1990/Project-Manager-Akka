using System.Collections.Immutable;
using Microsoft.AspNetCore.Components;
using MudBlazor;
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

    private IEnumerable<(int Count, ImmutableList<TData> List)> GroupToThree<TData>(IEnumerable<TData> enumerable)
    {
        int count = 0;
        var list = ImmutableList<TData>.Empty;

        foreach (TData data in enumerable)
        {
            list = list.Add(data);

            if(list.Count != 3)
                continue;

            count++;
            yield return (count, list);

            list = ImmutableList<TData>.Empty;
        }

        count++;
        yield return (count, list);
    }
}