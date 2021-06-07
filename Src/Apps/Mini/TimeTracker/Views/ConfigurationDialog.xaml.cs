using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Tauron;
using Tauron.Application;
using Tauron.Application.CommonUI.Commands;
using Tauron.Application.CommonUI.Dialogs;
using Tauron.Application.Wpf.Dialogs;
using TimeTracker.Managers;

namespace TimeTracker.Views
{
    /// <summary>
    /// Interaktionslogik für ConfigurationDialog.xaml
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

    public sealed class ConfigurationDialogModel : ObservableObject, IDisposable
    {
        private readonly CompositeDisposable _disposer = new();

        private string _monthHours = string.Empty;
        private string _minusShortTimeHours = string.Empty;
        private string _dailyHours = string.Empty;

        public string MonthHours
        {
            get => _monthHours;
            set => SetProperty(ref _monthHours, value);
        }

        public string MinusShortTimeHours
        {
            get => _minusShortTimeHours;
            set => SetProperty(ref _minusShortTimeHours, value);
        }

        public string DailyHours
        {
            get => _dailyHours;
            set => SetProperty(ref _dailyHours, value);
        }

        public ICommand Ok { get; }

        public ConfigurationDialogModel(IObserver<Unit> close, ConfigurationManager manager)
        {
            MonthHours = manager.MonthHours.ToString();
            MinusShortTimeHours = manager.MinusShortTimeHours.ToString();
            DailyHours = manager.DailyHours.ToString();

            Ok = new SimpleReactiveCommand()
               .Finish(obs => (from trigger in obs
                               let one = TryUpdate(MonthHours, i => manager.MonthHours = i)
                               let two = TryUpdate(MinusShortTimeHours, i => manager.MinusShortTimeHours = i)
                               let three = TryUpdate(DailyHours, i => manager.DailyHours = i)
                               select trigger)
                          .Subscribe(close))
                .DisposeWith(_disposer);

            (from newValue in this.WhenAny(() => MonthHours)
             let result = Try.From(() => int.Parse(newValue))
             where result.IsSuccess
             select result.Get())
               .Subscribe(i =>
                          {
                              try
                              {
                                  MinusShortTimeHours = (i * 0.10d).ToString("F0");
                                  DailyHours = (i / 20d).ToString("F0");
                              }
                              catch
                              {
                                  // ignored
                              }
                          })
               .DisposeWith(_disposer);
        }

        private bool TryUpdate(string value, Action<int> setter)
        {
            if (!int.TryParse(value, out var i) || i <= 0) return false;
            
            setter(i);
            return true;

        }

        public void Dispose() => _disposer.Dispose();
    }
}
