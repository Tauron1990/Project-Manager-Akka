using Microsoft.Extensions.DependencyInjection;

namespace Tauron.Modules;

public interface IModule
{
    void Load(IServiceCollection collection);
}