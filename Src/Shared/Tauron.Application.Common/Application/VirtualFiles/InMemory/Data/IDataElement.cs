using System;
using MHLab.Pooling;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public interface IDataElement : IDisposable, IPoolable
{
    string Name { get; set; }
}
    
public abstract class DataElementBase : IDataElement
{
    public string Name { get; set; } = string.Empty;

    public DateTime ModifyDate { get; set; }

    public DateTime CreationDate { get; set; }

    public virtual void Dispose()
    {
        ModifyDate = DateTime.MinValue;
        CreationDate = DateTime.MinValue;
    }

    public abstract void Recycle();
}