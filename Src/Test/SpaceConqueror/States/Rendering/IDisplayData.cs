using Spectre.Console.Rendering;

namespace SpaceConqueror.States.Rendering;

public interface IDisplayData
{
    int Order { get; }

    IRenderable ToRender { get; }
}