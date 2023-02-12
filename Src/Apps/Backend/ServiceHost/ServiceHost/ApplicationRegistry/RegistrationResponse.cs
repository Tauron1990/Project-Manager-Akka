using System;
using Tauron.Application.Master.Commands;
using Tauron.Application.Master.Commands.Administration.Host;

namespace ServiceHost.ApplicationRegistry
{
    public sealed record NewRegistrationRequest(string SoftwareName, AppName Name, string Path, SimpleVersion Version, AppType AppType, string ExeFile);

    public sealed record RegistrationResponse(bool Scceeded, Exception? Error);
}