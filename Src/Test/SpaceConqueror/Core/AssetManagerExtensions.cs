namespace SpaceConqueror.Core;

public static class AssetManagerExtensions
{
    public static string GetString(this AssetManager manager, string input)
        => input.Length > 40 ? input : manager.TryGet<string>(input).Map(input);
}