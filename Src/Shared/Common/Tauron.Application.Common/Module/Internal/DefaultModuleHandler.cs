using System;
using Microsoft.Extensions.DependencyInjection;

namespace Tauron.Module.Internal;

public sealed class DefaultModuleHandler : IModuleHandler
{
    public void Handle(IServiceCollection collection, IModule module) => module.Load(collection);
}