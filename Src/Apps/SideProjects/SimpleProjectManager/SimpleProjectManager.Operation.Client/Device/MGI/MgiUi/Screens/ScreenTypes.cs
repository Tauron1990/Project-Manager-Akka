using SimpleProjectManager.Operation.Client.Device.UiHelper;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Operation.Client.Device.MGI.MgiUi.Screens;

public static class ScreenTypes
{
    public const string Configuration = nameof(Configuration);

    public const string UVControl = nameof(UVControl);

    public static UiManagerHelper Init(UiManagerHelper managerHelper, UiConfiguration uiConfiguration)
    {
        return managerHelper
            .WithScreen(Configuration, SimpleLazy.Create(uiConfiguration, ConfigurationScreen.Create));
    }
}