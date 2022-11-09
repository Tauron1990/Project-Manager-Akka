using Tauron.Application.VirtualFiles;
using Tauron.Application.Workshop.Driver;

namespace Tauron.Application.SoftwareRepo;

public sealed class RepoFactory : IRepoFactory
{
    public SoftwareRepository Create(IDriverFactory factory, IDirectory path)
    {
        var temp = new SoftwareRepository(factory, path);
        temp.InitNew();

        return temp;
    }

    public SoftwareRepository Read(IDriverFactory factory, IDirectory path)
    {
        var temp = new SoftwareRepository(factory, path);
        temp.Init();

        return temp;
    }

    public bool IsValid(IDirectory path)
        => path.GetFile(SoftwareRepository.FileName).Exist;

    // public SoftwareRepository Create(IActorRefFactory factory, string path)
    //     => Create(factory, new VirtualFileFactory().CrerateLocal(path));
    //
    // public SoftwareRepository Read(IActorRefFactory factory, string path)
    //     => Read(factory, new VirtualFileFactory().CrerateLocal(path));
    //
    // public bool IsValid(string path)
    //     => IsValid(new VirtualFileFactory().CrerateLocal(path));
}