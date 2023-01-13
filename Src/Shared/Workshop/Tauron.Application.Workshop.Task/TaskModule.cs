using Microsoft.Extensions.DependencyInjection;
using Tauron.Application.Workshop.Driver;

namespace Tauron.Application.Workshop;

public sealed class TaskModule : IModule
{
    public void Load(IServiceCollection collection)
        => collection.AddTransient<IDriverFactory, TaskDriverFactory>();
}