﻿using Autofac;
using SimpleProjectManager.Operation.Client.Shared;
using SimpleProjectManager.Server.Controllers.FileUpload;
using SimpleProjectManager.Server.Core.JobManager;
using Tauron.Application.AkkaNode.Bootstrap;

namespace SimpleProjectManager.Server;

public sealed class MainModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {        
        var runner = new ClientRunner();
        runner.ApplyClientServices(builder);
        
        builder.RegisterStartUpAction<ClusterJoinSelf>();
        builder.RegisterStartUpAction<JobManagerRegistrations>();

        builder.RegisterType<FileUploadTransaction>().InstancePerDependency();
        
        base.Load(builder);
    }
}