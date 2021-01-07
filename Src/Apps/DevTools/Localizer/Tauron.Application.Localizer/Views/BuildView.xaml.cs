using System.Windows.Media;
using Tauron.Application.CommonUI;
using Tauron.Application.Localizer.UIModels;

namespace Tauron.Application.Localizer.Views
{
    /// <summary>
    /// Interaktionslogik für BuildView.xaml
    /// </summary>
    public partial class BuildView
    {
        public BuildView(IViewModel<BuildViewModel> model)
            : base(model)
        {
            InitializeComponent();
            Background = Brushes.Transparent;
        }
    }
}
