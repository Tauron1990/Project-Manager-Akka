using System;
using System.Collections.Generic;

namespace Tauron.Application.Localizer.UIModels.Services
{
    public interface IOperationManager
    {
        IEnumerable<RunningOperation> RunningOperations { get; }

        IObservable<OperationController> StartOperation(string name);

        IObservable<OperationController?> Find(string id);

        IObservable<bool> ShouldClear();

        void Clear();


        IObservable<bool> ShouldCompledClear();

        void CompledClear();
    }
}