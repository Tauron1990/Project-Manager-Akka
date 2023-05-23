using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Operation.Client.Device.UiHelper;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Operation.Client.Device.MGI.MgiUi.Screens;

public static class ScreenTypes
{
    public const string Configuration = nameof(Configuration);

    public const string MainControl = nameof(MainControl);

    public static UiManagerHelper Init(UiManagerHelper managerHelper, UiConfiguration uiConfiguration)
    {
        var loggerFactory = managerHelper.Services.GetRequiredService<ILoggerFactory>();
        
        return managerHelper
            .WithScreen(Configuration, SimpleLazy.Create((uiConfiguration, loggerFactory), ConfigurationScreen.Create))
            .WithScreen(MainControl, SimpleLazy.Create((uiConfiguration, managerHelper.Services), MainScreen.Create));
    }
}