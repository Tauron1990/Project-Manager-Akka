﻿using Autofac;
using Tauron.Application.AkkaNode.Bootstrap;

namespace SimpleProjectManager.Server;

public sealed class MainModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterStartUpAction<ClusterJoinSelf>();
        base.Load(builder);
    }
}