using JetBrains.Annotations;

namespace Tauron.Application.CommonUI.AppCore;

[PublicAPI]
public enum ShutdownMode
{
    OnLastWindowClose,
    OnMainWindowClose,
    OnExplicitShutdown,
}