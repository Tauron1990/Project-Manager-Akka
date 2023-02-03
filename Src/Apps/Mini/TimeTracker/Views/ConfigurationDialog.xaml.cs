using System.Reactive;
using System.Threading.Tasks;
using Tauron.Application.CommonUI.Dialogs;
using Tauron.Application.Wpf.Dialogs;
using TimeTracker.Managers;

namespace TimeTracker.Views
{
    /// <summary>
    ///     Interaktionslogik für ConfigurationDialog.xaml
    /// </summary>
    public partial class ConfigurationDialog : IBaseDialog<Unit, ConfigurationManager>
    {
        public ConfigurationDialog()
        {
            InitializeComponent();
        }

        public Task<Unit> Init(ConfigurationManager initalData)
            => this.MakeObsTask<Unit>(o => new ConfigurationDialogModel(o, initalData));
    }
}