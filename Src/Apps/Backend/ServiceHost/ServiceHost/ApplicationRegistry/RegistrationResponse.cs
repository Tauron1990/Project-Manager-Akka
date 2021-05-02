using System;
using Tauron.Application.Master.Commands.Administration.Host;

namespace ServiceHost.ApplicationRegistry
{
    public sealed record NewRegistrationRequest(string Name, string Path, int Version, AppType AppType, string ExeFile);

    public sealed record RegistrationResponse(bool Scceeded, Exception? Error);
}