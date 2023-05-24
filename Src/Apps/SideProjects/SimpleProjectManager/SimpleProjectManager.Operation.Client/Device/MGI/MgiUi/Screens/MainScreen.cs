using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Operation.Client.Device.UiHelper;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Operation.Client.Device.MGI.MgiUi.Screens;

public class MainScreen : ScreenModelBase
{
    public static IScreenModel Create((UiConfiguration Configuration, IServiceProvider ServiceProvider) parm)
        => new MainScreen(parm.Configuration, parm.ServiceProvider, parm.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<MainScreen>());

    public MainScreen(UiConfiguration configuration, IServiceProvider serviceProvider, ILogger logger) : base(logger)
    {
        
    }

    protected override void InitModel()
    {
        
    }

    protected override DeviceUiGroup CreateInitialUI() => DeviceUi.Text("To be Implemented");
}