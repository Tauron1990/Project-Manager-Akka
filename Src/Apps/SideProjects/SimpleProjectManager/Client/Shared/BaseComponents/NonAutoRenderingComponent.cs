using Microsoft.AspNetCore.Components;

namespace SimpleProjectManager.Client.Shared.BaseComponents;

public class NonAutoRenderingComponent : DisposableComponent
{
    public RenderingManager RenderingManager { get; } = new();

    protected override void OnInitialized()
    {
        RenderingManager.Init(StateHasChanged, InvokeAsync);
        base.OnInitialized();
    }

    protected override bool ShouldRender()
        => RenderingManager.CanRender;
}