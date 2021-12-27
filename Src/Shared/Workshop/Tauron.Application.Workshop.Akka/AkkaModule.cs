using Microsoft.Extensions.DependencyInjection;
using Tauron.Application.Workshop.Driver;

namespace Tauron.Application.Workshop;

public class AkkaModule : IModule
{
    public void Load(IServiceCollection collection)
        => collection.AddTransient<IDriverFactory, AkkaDriverFactory>();
}