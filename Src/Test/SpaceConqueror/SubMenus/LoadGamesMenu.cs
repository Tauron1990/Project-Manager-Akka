using SpaceConqueror.States;

namespace SpaceConqueror.SubMenus;

public class LoadGamesMenu
{
    private readonly GlobalState _globalState;

    public LoadGamesMenu(GlobalState globalState)
        => _globalState = globalState;

    public async ValueTask<bool> Show()
        => true;
}