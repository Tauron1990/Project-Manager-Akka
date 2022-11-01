using SpaceConqueror.Core;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace SpaceConqueror.States.Rendering;

public sealed class AssetDisplayData : IDisplayData
{
    public int Order { get; }
    public IRenderable ToRender { get; }

    public AssetDisplayData(int order, AssetManager manager, string name)
    {
        Order = order;
        ToRender = new Markup(manager.Get<string>(name));
    }
}