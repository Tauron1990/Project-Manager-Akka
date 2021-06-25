using System;
using Tauron.Application.CommonUI.UI;

namespace Tauron.Application.Blazor.UI
{
    public interface IInternalbinding : IDisposable
    {
        void Update(DeferredSource source, Action updateState);
    }
}