using System.Threading.Tasks;
using Tauron.Application.CommonUI.Dialogs;
using Tauron.Application.Wpf.Dialogs;
using TimeTracker.Managers;

namespace TimeTracker.Views
{
    /// <summary>
    ///     Interaktionslogik für AddEntryDialog.xaml
    /// </summary>
    public partial class AddEntryDialog : IBaseDialog<AddEntryResult, AddEntryParameter>
    {
        private readonly SystemClock _clock;
        private readonly HolidayManager _manager;

        public AddEntryDialog(HolidayManager manager, SystemClock clock)
        {
            _manager = manager;
            _clock = clock;
            InitializeComponent();
        }

        public Task<AddEntryResult> Init(AddEntryParameter initalData)
            => this.MakeObsTask<AddEntryResult>(o => new AddEntryDialogModel(o, initalData, _manager, _clock));
    }
}