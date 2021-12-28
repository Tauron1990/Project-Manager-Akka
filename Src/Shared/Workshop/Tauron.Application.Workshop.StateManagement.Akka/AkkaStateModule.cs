using Microsoft.Extensions.DependencyInjection;
using Tauron.Application.Workshop.StateManagement.Akka.Builder;
using Tauron.Application.Workshop.StateManagement.Builder;

namespace Tauron.Application.Workshop.StateManagement.Akka;

public class AkkaStateModule : IModule
{
    public void Load(IServiceCollection collection)
    {
        collection.AddTransient<IStateInstanceFactory, FeatureBasedState>();
    }
}