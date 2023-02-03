using System;
using System.Threading.Tasks;
using Tauron.Application.CommonUI.Dialogs;
using Tauron.Application.Wpf.Dialogs;

namespace TimeTracker.Views
{
    /// <summary>
    ///     Interaktionslogik für VacationDialog.xaml
    /// </summary>
    public partial class VacationDialog : IBaseDialog<DateTime[]?, DateTime>
    {
        public VacationDialog()
        {
            InitializeComponent();
        }

        public Task<DateTime[]?> Init(DateTime initalData)
            => this.MakeObsTask<DateTime[]?>(o => new VacationDialogModel(o, initalData, Calendar.SelectedDates));
    }
}