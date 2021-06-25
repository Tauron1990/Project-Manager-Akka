using System.Reactive.Linq;
using System.Windows.Input;
using Autofac;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Model;

namespace BlazorTest.ViewModel
{
    public class CounterViewModel : UiActor
    {
        public UIProperty<int> Counter { get; }

        public UIProperty<ICommand> Command { get; }

        public CounterViewModel(ILifetimeScope lifetimeScope, IUIDispatcher dispatcher) 
            : base(lifetimeScope, dispatcher)
        {
            Counter = RegisterProperty<int>(nameof(Counter))
               .WithDefaultValue(1);

            Command = NewCommad
                     .WithFlow(obs => (from _ in obs
                                      from current in Counter.Take(1) 
                                      select current + 1)
                                  .Subscribe(Counter))
               .ThenRegister(nameof(Command));
        }
    }
}