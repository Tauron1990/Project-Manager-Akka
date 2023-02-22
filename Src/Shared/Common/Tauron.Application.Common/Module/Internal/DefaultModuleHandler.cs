using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tauron.Module.Internal;

public sealed class DefaultModuleHandler : IModuleHandler
{
    public void Handle(IHostBuilder collection, IModule module) => collection.ConfigureServices(module.Load);
}