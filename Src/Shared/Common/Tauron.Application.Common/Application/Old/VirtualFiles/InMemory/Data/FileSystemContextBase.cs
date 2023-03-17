using System;
using System.Reactive.PlatformServices;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public abstract record FileSystemContextBase<TData>(InMemoryRoot Root, TData? Data, PathInfo Path, ISystemClock Clock, InMemoryFileSystem RootSystem)
    where TData : class, IDataElement
{
    public TData ActualData
    {
        get
        {
            if(Data is null)
                throw new InvalidOperationException("Element does not Exist");

            return Data;
        }
    }
}