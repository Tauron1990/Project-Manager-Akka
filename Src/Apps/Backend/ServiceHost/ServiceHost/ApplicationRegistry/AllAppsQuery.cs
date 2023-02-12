using Tauron.Application.Master.Commands;

namespace ServiceHost.ApplicationRegistry
{
    public sealed record AllAppsQuery;

    public sealed record AllAppsResponse(AppName[] Apps);
}