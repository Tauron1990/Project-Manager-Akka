using System.Collections.Generic;
using System.Globalization;

namespace Tauron.Application.Localizer.UIModels.Views
{
    public interface ILanguageSelectorDialog : IBaseDialog<AddLanguageDialogResult?, IEnumerable<CultureInfo>> { }
}