﻿using Hyperion.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleProjectManager.Server.Controllers.FileUpload;
using SimpleProjectManager.Server.Core.JobManager;
using Tauron.Application.AkkaNode.Bootstrap;

namespace SimpleProjectManager.Server;

[UsedImplicitly]
public sealed class MainModule : IModule
{
    public void Load(IServiceCollection collection)
        => collection.TryAddScoped<FileUploadTransaction>();
}