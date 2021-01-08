using Avalonia.Controls;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.UI;

namespace Tauron.Application.Avalonia.UI
{
    public sealed class WindowControlLogic : ControlLogicBase<Window>
    {
        public WindowControlLogic(Window userControl, IViewModel model) : base(userControl, model)
        {
            userControl.SizeToContent = SizeToContent.Manual;
            userControl.ShowInTaskbar = true;
            userControl.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        protected override void WireUpUnloaded()
        {
            UserControl.Closed += (_, _) => UserControlOnUnloaded();
        }
    }
}