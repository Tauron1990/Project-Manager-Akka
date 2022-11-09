using Spectre.Console.Rendering;

namespace SpaceConqueror.States.Rendering;

public sealed class StaticDisplayData : IDisplayData
{
    public StaticDisplayData(int order, IRenderable toRender)
    {
        Order = order;
        ToRender = toRender;
    }

    public int Order { get; }
    public IRenderable ToRender { get; }
}