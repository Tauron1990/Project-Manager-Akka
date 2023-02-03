using Tauron.Application.CommonUI;
using Tauron.Application.Localizer.DataModel;

namespace Tauron.Application.Localizer.UIModels
{
    public sealed record ProjectViewContainer(Project Project, IViewModel Model);
}