using Autofac;
using JetBrains.Annotations;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Model;

namespace TimeTracker.ViewModels
{
    public sealed class MainWindowViewModel : UiActor
    {
        public MainWindowViewModel(ILifetimeScope lifetimeScope, IUIDispatcher dispatcher) : base(lifetimeScope, dispatcher)
        {
        }
    }
}