using JetBrains.Annotations;
using Tauron.Application.VirtualFiles;
using Tauron.Application.Workshop.Driver;

namespace Tauron.Application.SoftwareRepo;

[PublicAPI]
public interface IRepoFactory
{
    SoftwareRepository Create(IDriverFactory factory, IDirectory path);
    SoftwareRepository Read(IDriverFactory factory, IDirectory path);
    bool IsValid(IDirectory path);

    // SoftwareRepository Create(IActorRefFactory factory, string path);
    //
    // SoftwareRepository Read(IActorRefFactory factory, string path);
    // bool IsValid(string path);
}