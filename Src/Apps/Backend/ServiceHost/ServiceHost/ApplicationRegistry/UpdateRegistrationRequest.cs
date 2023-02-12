using Tauron.Application.Master.Commands;

namespace ServiceHost.ApplicationRegistry
{
    public sealed record UpdateRegistrationRequest(AppName Name);
}