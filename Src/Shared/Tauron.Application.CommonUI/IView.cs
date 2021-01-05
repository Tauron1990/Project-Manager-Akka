using System;
using Tauron.Application.CommonUI.Helper;

namespace Tauron.Application.CommonUI
{
    public interface IView : IBinderControllable
    {
        event Action? ControlUnload;
    }
}