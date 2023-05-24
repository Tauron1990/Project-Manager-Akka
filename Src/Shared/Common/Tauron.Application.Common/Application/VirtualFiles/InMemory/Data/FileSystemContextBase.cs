using System.Reactive.PlatformServices;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public abstract record FileSystemContextBase<TData>(InMemoryRoot Root, DirectoryContext? Parent, TData? Data, PathInfo Path, ISystemClock Clock, InMemoryFileSystem RootSystem)
    where TData : class, IDataElement
{
    public Result<TData> ActualData
    {
        get
        {
            if(Data is null)
                return new Error("Element does not Exist");

            return Data;
        }
    }
}