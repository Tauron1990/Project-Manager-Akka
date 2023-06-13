using System;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public abstract class DataElementBase : IDataElement
{
    public DateTime ModifyDate { get; set; }

    public string Name { get; set; } = string.Empty;

    public IDataElement SetName(string name)
    {
        Name = name;
        return this;
    }

    public virtual void Dispose()
    {
        ModifyDate = DateTime.MinValue;
    }
}