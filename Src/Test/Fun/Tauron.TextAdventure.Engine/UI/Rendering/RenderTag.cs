namespace Tauron.TextAdventure.Engine.UI.Rendering;

public readonly struct RenderTag
{
    public readonly string TagValue = string.Empty;

    public RenderTag(string tagValue)
        => TagValue = tagValue;
}