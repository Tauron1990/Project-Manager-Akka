using System;
using Akka.MGIHelper.UI;
using Akka.MGIHelper.UI.FanControl;
using Akka.MGIHelper.UI.MgiStarter;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Model;

namespace Akka.MGIHelper
{
    public sealed class MainWindowViewModel : UiActor
    {
        public MainWindowViewModel(IServiceProvider lifetimeScope, IUIDispatcher dispatcher, IViewModel<MgiStarterControlModel> mgiStarter, IViewModel<AutoFanControlModel> autoFanControl)
            : base(lifetimeScope, dispatcher)
        {
            this.RegisterViewModel("MgiStarter", mgiStarter);
            this.RegisterViewModel("FanControl", autoFanControl);

            NewCommad
               .WithExecute(ShowWindow<LogWindow>)
               .ThenRegister("OpenLogs");
        }
    }
}