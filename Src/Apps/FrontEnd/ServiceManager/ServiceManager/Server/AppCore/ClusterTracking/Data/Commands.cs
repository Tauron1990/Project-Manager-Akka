﻿using System.Reactive;
using Stl.CommandR;
using Tauron.Application.Master.Commands.ServiceRegistry;

namespace ServiceManager.Server.AppCore.ClusterTracking.Data
{
    public sealed record AddNodeCommad(string Url, string Name, string Status, string ServiceType) : ICommand<Unit>;

    public sealed record RemoveNodeCommand(string Url) : ICommand<Unit>;

    public sealed record UpdateStatusCommand(string Url, string Status) : ICommand<Unit>;

    public sealed record UpdateNameCommand(string Url, ServiceName Name, string ServiceType) : ICommand<Unit>;
}