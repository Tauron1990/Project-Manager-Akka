using System;
using MHLab.Pooling;

namespace Tauron.Application.VirtualFiles.InMemory.Data
{
    public interface IDataElement : IDisposable, IPoolable
    {
        
    }
}