using Tauron.Application.Master.Commands.Administration.Host;

namespace ServiceHost.Services.Impl
{
    public record InternalStopApp(AppState Restart)
    {
        public InternalStopApp()
            : this(Restart: AppState.NotRunning) { }
    }
}