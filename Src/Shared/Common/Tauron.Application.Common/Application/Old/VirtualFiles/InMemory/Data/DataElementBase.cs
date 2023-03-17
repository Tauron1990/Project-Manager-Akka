using System;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public abstract class DataElementBase : IDataElement
{
    public DateTime ModifyDate { get; init; }

    public string Name { get; init; } = string.Empty;
    public abstract void Dispose();
}