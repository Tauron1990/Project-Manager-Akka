using System.Collections.Generic;
using Tauron.Application.CommonUI.Dialogs;

namespace Tauron.Application.Localizer.UIModels.Views
{
    public interface IProjectNameDialog : IBaseDialog<NewProjectDialogResult, IEnumerable<string>> { }
}