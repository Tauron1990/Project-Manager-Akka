using System.Collections.Generic;
using System.Globalization;
using Tauron.Application.CommonUI.Dialogs;

namespace Tauron.Application.Localizer.UIModels.Views
{
    public interface ILanguageSelectorDialog : IBaseDialog<AddLanguageDialogResult?, IEnumerable<CultureInfo>>
    {
    }
}