using Microsoft.Extensions.Hosting;

namespace Tauron.Module.Internal;

public interface IModuleHandler
{
    void Handle(IHostBuilder collection, IModule module);
}