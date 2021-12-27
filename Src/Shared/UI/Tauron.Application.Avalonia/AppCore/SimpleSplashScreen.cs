using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;

namespace Tauron.Application.Avalonia.AppCore
{
    public sealed class SimpleSplashScreen<TSplash> : ISplashScreen
        where TSplash : Window, IWindow, new()
    {
        public IWindow Window => new TSplash();
    }
}