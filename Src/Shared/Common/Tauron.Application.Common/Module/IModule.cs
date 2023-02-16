using Microsoft.Extensions.DependencyInjection;

namespace Tauron;

public interface IModule
{
    void Load(IServiceCollection collection);
}