using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Tauron;
using Tauron.Application.CommonUI.Commands;
using Tauron.Application.CommonUI.Dialogs;
using Tauron.Application.CommonUI.Model;
using Tauron.Application.Wpf.Dialogs;
using TimeTracker.Data;
using TimeTracker.Managers;

namespace TimeTracker.Views
{
    /// <summary>
    /// Interaktionslogik für AddEntryDialog.xaml
    /// </summary>
    public partial class AddEntryDialog : IBaseDialog<AddEntryResult, AddEntryParameter>
    {
        private readonly HolidayManager _manager;
        private readonly SystemClock _clock;

        public AddEntryDialog(HolidayManager manager, SystemClock clock)
        {
            _manager = manager;
            _clock = clock;
            InitializeComponent();
        }

        public Task<AddEntryResult> Init(AddEntryParameter initalData) 
            => this.MakeObsTask<AddEntryResult>(o => new AddEntryDialogModel(o, initalData, _manager, _clock));
    }

    public sealed class AddEntryDialogModel : ObservableErrorObject
    {
        public int MaxDay { get; }

        public string CurrentMonth { get; }

        public int Day
        {
            get => GetValue(0);
            set => SetValue(value);
        }

        public string Start
        {
            get => GetValue(string.Empty);
            set => SetValue(value);
        }

        public string Finish
        {
            get => GetValue(string.Empty);
            set => SetValue(value);
        }

        public ICommand Cancel { get; }

        public ICommand Ok { get; }

        public AddEntryDialogModel(IObserver<AddEntryResult> resultObserver, AddEntryParameter parameter, HolidayManager manager, SystemClock clock)
        {
            MaxDay = clock.DaysInCurrentMonth(parameter.CurrentMonth);
            CurrentMonth = parameter.CurrentMonth.ToString("d");
            Day = clock.NowDate.AddDays(1).Day;
            Start = string.Empty;
            Finish = string.Empty;

            var blocked = parameter.BlockedDays.ToArray();
            AddValidation(
                () => Day,
                obs => from day in obs
                       where blocked.Contains(day)
                       select "Tag ist schon belegt");
            AddValidation(
                () => Start,
                sobs => from time in sobs
                        let ty = TimeSpan.TryParse(time, out _)
                        select ty ? null : "Kann nicht in Zeitpunkt Umgewandelt werden");
            AddValidation(
                () => Finish,
                sobs => from time in sobs
                        let ty = string.IsNullOrWhiteSpace(time) || TimeSpan.TryParse(time, out _)
                        select ty ? null : "Kann nicht in Zeitpunkt Umgewandelt werden");

            Cancel = new SimpleReactiveCommand()
                    .Finish(obs => obs.Select(_ => new CancelAddEntryResult()).Subscribe(resultObserver))
                    .DisposeWith(Disposer);

            Ok = new SimpleReactiveCommand(from _ in ErrorsChanged
                                           select ErrorCount == 0)
                .Finish(obs => (from _ in obs
                                let date = new DateTime(parameter.CurrentMonth.Year, parameter.CurrentMonth.Month, Day)
                                from isHoliday in manager.IsHoliday(date, Day)
                                let data = new ProfileEntry(date, TimeSpan.Parse(Start), string.IsNullOrWhiteSpace(Finish) ? null : TimeSpan.Parse(Finish), isHoliday ? DayType.Holiday : DayType.Normal)
                                select new NewAddEntryResult(data))
                           .Subscribe(resultObserver))
                .DisposeWith(Disposer);
        }
    }
}
