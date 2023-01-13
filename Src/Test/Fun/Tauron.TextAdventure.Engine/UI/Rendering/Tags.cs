namespace Tauron.TextAdventure.Engine.UI.Rendering;

public static class Tags
{
    public static RenderTag MainMenu { get; } = new(nameof(MainMenu));

    public static RenderTag NewGame { get; } = new(nameof(NewGame));

    public static RenderTag LoadGame { get; } = new(nameof(LoadGame));
}