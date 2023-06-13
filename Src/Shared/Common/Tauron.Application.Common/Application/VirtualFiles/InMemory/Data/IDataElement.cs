using System;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public interface IDataElement : IDisposable
{
    string Name { get; }

    IDataElement SetName(string name);
}