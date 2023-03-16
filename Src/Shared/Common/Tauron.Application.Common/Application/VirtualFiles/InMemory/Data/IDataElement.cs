using System;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

[PublicAPI]
public interface IDataElement : IDisposable
{
    string Name { get; init; }
}