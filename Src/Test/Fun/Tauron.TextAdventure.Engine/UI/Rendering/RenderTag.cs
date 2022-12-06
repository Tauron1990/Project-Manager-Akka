namespace Tauron.TextAdventure.Engine.UI.Rendering;

public readonly record struct RenderTag(string TagValue)
{
    public bool IsEmpty => string.IsNullOrEmpty(TagValue);
}