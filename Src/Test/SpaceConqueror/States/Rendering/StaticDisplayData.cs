using Spectre.Console.Rendering;

namespace SpaceConqueror.States.Rendering;

public sealed class StaticDisplayData : IDisplayData
{
    public int Order { get; }
    public IRenderable ToRender { get; }

    public StaticDisplayData(int order, IRenderable toRender)
    {
        Order = order;
        ToRender = toRender;
    }
}