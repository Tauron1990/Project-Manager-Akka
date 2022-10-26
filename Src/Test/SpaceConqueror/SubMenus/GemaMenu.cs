namespace SpaceConqueror.SubMenus;

public sealed class GemaMenu
{
    private readonly GameManager _manager;

    public GemaMenu(GameManager manager)
        => _manager = manager;

    public ValueTask RunGame()
        => default;
}