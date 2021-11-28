﻿
using Autofac;

namespace SimpleProjectManager.Server.Core.Services
{
    public sealed class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FileContentManager>().SingleInstance();
            builder.RegisterType<CommitRegistrationTransaction>().InstancePerDependency();
            builder.RegisterType<PreRegisterTransaction>().InstancePerDependency();
            base.Load(builder);
        }
    }
}
