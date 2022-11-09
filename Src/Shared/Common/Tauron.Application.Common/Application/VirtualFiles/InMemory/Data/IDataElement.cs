using System;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public interface IDataElement : IDisposable
{
    string Name { get; set; }
}

public abstract class DataElementBase : IDataElement
{
    public DateTime ModifyDate { get; set; }

    public DateTime CreationDate { get; set; }
    public string Name { get; set; } = string.Empty;

    public virtual void Dispose()
    {
        ModifyDate = DateTime.MinValue;
        CreationDate = DateTime.MinValue;
    }
}