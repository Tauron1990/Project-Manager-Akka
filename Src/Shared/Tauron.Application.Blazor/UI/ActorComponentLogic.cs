using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.UI;

namespace Tauron.Application.Blazor.UI
{
    public sealed class ActorComponentLogic : ControlLogicBase<ComponentUIObject>
    {
        public ActorComponentLogic(ComponentUIObject userControl, IViewModel model)
            : base(userControl, model) { }
    }
}