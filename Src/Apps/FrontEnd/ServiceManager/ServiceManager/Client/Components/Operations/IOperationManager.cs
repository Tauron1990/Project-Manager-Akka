using System;

namespace ServiceManager.Client.Components.Operations
{
    public interface IOperationManager
    {
        IDisposable Start();

        IDisposable Subscribe(IObserver<bool> action);
    }
}