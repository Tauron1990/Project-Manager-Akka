using System.Threading.Tasks;

namespace Tauron.Application.Blazor;

public class NonAutoRenderingComponent : DisposableComponent
{
    public RenderingManager RenderingManager { get; } = new();

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await RenderingManager.StateHasChangedAsync();
    }

    protected override void OnInitialized()
    {
        RenderingManager.Init(StateHasChanged, InvokeAsync);
        base.OnInitialized();
    }

    protected override bool ShouldRender()
        => RenderingManager.CanRender;
}