using System;
using System.Collections.Generic;

namespace Tauron.Application.Localizer.UIModels.Services
{
    public interface IOperationManager
    {
        public IObservable<RunningOperation> OperationFailed { get; }

        IEnumerable<RunningOperation> RunningOperations { get; }

        IObservable<OperationController> StartOperation(string name);

        IObservable<OperationController?> Find(string id);

        IObservable<bool> ShouldClear();

        void Clear();


        IObservable<bool> ShouldCompledClear();

        void CompledClear();
    }
}