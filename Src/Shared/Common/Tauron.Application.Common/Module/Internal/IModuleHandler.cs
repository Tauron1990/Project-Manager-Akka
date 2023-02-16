using System;
using Microsoft.Extensions.DependencyInjection;

namespace Tauron.Module.Internal;

public interface IModuleHandler
{
    void Handle(IServiceCollection collection, IModule module);
}