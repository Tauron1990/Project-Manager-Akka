using System;
using JetBrains.Annotations;

namespace Tauron.Application.CommonUI.AppCore;

[PublicAPI]
public interface IUIApplication
{
    ShutdownMode ShutdownMode { get; set; }

    IUIDispatcher AppDispatcher { get; }
    event EventHandler? Startup;

    void Shutdown(int returnValue);
    int Run();
}