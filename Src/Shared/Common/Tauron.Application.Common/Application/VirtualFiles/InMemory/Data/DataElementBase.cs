using System;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public abstract class DataElementBase : IDataElement
{
    public DateTime ModifyDate { get; set; }

    public string Name { get; set; } = string.Empty;

    public virtual void Dispose()
    {
        ModifyDate = DateTime.MinValue;
    }
}