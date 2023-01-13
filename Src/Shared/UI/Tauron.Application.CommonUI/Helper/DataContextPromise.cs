using System;

namespace Tauron.Application.CommonUI.Helper;

public abstract class DataContextPromise
{
    public abstract void OnUnload(Action unload);

    public abstract void OnContext(Action<IViewModel, IView> modelAction);

    public abstract void OnNoContext(Action action);
}