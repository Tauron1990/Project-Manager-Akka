using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;

namespace Tauron.Application.Wpf.AppCore;

public sealed class SimpleSplashScreen<TSplash> : ISplashScreen
    where TSplash : System.Windows.Window, IWindow, new()
{
    public IWindow Window => new TSplash();
}