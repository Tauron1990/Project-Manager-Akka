using System;
using JetBrains.Annotations;

namespace Tauron.Application.CommonUI.AppCore;

[PublicAPI]
public interface IMainWindow : IWindowProvider
{
    event EventHandler? Shutdown;
}