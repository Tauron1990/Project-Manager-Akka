using System;
using Tauron.Application.CommonUI.Helper;

namespace Tauron.Application.CommonUI;

public interface IView : IBinderControllable
{
    #pragma warning disable MA0046
    event Action? ControlUnload;
    #pragma warning restore MA0046
}