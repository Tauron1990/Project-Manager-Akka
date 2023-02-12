using JetBrains.Annotations;
using Tauron.Application.Master.Commands;

namespace ServiceHost.Services
{
    [PublicAPI]
    public sealed record StopResponse(AppName Name, bool Error);
}