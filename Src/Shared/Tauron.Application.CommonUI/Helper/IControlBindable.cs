using System;

namespace Tauron.Application.CommonUI.Helper
{
    public interface IControlBindable
    {
        IDisposable Bind(IUIObject root, IUIObject affectedObject, object dataContext);
    }
}