namespace SimpleProjectManager.Client.Shared.BaseComponents;

public class NonAutoRenderingComponent : DisposableComponent
{
    protected RenderingManager RenderingManager { get; } = new();

    protected override void OnInitialized()
    {
        RenderingManager.Init(StateHasChanged, InvokeAsync);
        base.OnInitialized();
    }

    protected override bool ShouldRender()
        => RenderingManager.CanRender;
}