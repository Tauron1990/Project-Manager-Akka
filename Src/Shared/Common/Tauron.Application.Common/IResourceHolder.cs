using System;

namespace Tauron;

public interface IResourceHolder : IDisposable
{
    void AddResource(IDisposable res);
    void RemoveResource(IDisposable res);
}