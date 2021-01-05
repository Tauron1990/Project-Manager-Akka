using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.UI;

namespace Tauron.Application.Avalonia.UI
{
    public sealed class UserControlLogic : ControlLogicBase<UserControl>
    {
        public UserControlLogic(UserControl userControl, IViewModel model) : base(userControl, model)
        {
        }
    }
}