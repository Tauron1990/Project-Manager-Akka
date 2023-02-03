using System.Threading.Tasks;
using Tauron.Application.CommonUI.Dialogs;
using Tauron.Application.Wpf.Dialogs;
using TimeTracker.Data;
using TimeTracker.Managers;

namespace TimeTracker.Views
{
    /// <summary>
    ///     Interaktionslogik für CorrectionDialog.xaml
    /// </summary>
    public partial class CorrectionDialog : IBaseDialog<CorrectionResult, ProfileEntry>
    {
        private readonly HolidayManager _manager;

        public CorrectionDialog(HolidayManager manager)
        {
            _manager = manager;
            InitializeComponent();
        }

        public Task<CorrectionResult> Init(ProfileEntry initalData)
            => this.MakeObsTask<CorrectionResult>(o => new CorrectionDialogModel(o, initalData, _manager));
    }
}