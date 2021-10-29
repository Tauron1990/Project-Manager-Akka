﻿using Akka.Actor;
using Tauron;
using Tauron.Application.AkkaNode.Bootstrap;

namespace SimpleProjectManager.Server.Core.Projections.Core;

public sealed class ProjectionInitializer : IStartUpAction
{
    private readonly ActorSystem _system;
    private readonly IEnumerable<IInitializeProjection> _projections;

    public ProjectionInitializer(ActorSystem system, IEnumerable<IInitializeProjection> projections)
    {
        _system = system;
        _projections = projections;
    }

    public void Run()
        => _projections.Foreach(p => p.Initialize(_system));
}