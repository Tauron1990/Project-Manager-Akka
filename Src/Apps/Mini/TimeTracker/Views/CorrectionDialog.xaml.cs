using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    /// Interaktionslogik für CorrectionDialog.xaml
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

    public sealed class CorrectionDialogModel : ObservableErrorObject
    {
        public string StartTime
        {
            get => GetValue(string.Empty);
            set => SetValue(value);
        }

        public string FinishTime
        {
            get => GetValue(string.Empty);
            set => SetValue(value);
        }

        public string Date
        {
            get => GetValue(string.Empty);
            set => SetValue(value);
        }

        public SimpleReactiveCommand Apply { get; }

        public SimpleReactiveCommand Cancel { get; }

        public SimpleReactiveCommand Delete { get; }

        public CorrectionDialogModel(IObserver<CorrectionResult> observer, ProfileEntry initial, HolidayManager manager)
        {
            Date = initial.Date.ToString("d");
            FinishTime = initial.Finish?.ToString(@"hh\:mm") ?? string.Empty;
            StartTime = initial.Start?.ToString(@"hh\:mm") ?? string.Empty;

            AddValidation(
                () => StartTime,
                sobs => from time in sobs
                        let ty = TimeSpan.TryParse(time, out _)
                        select ty ? null : "Kann nicht in Zeitpunkt Umgewandelt werden");
            AddValidation(
                () => FinishTime,
                sobs => from time in sobs
                        let ty = string.IsNullOrWhiteSpace(time) || TimeSpan.TryParse(time, out _)
                        select ty ? null : "Kann nicht in Zeitpunkt Umgewandelt werden");

            AddValidation(
                () => Date,
                sobs => from time in sobs
                        let ty = DateTime.TryParse(time, out _)
                        select ty ? null : "Kann nicht in Datum Umgewandelt werden");

            Apply = new SimpleReactiveCommand((from _ in ErrorsChanged
                                               select ErrorCount == 0).StartWith(ErrorCount == 0))
                   .Finish(obs => (from _ in obs
                                   let date = DateTime.Parse(Date).Date
                                   let start = TimeSpan.Parse(StartTime)
                                   let end = string.IsNullOrWhiteSpace(FinishTime) ? Timeout.InfiniteTimeSpan : TimeSpan.Parse(FinishTime)
                                   from isHoliday in manager.IsHoliday(date, date.Day)
                                   select new UpdateCorrectionResult(new ProfileEntry(date, start, end == Timeout.InfiniteTimeSpan ? null : end, isHoliday ? DayType.Holiday : DayType.Normal)))
                              .Subscribe(observer))
                   .DisposeWith(Disposer);

            Cancel = new SimpleReactiveCommand()
                    .Finish(o => o.Select(_ => new CancelCorrectionResult()).Subscribe(observer))
                    .DisposeWith(Disposer);

            Delete = new SimpleReactiveCommand()
                    .Finish(o => o.Select(_ => new DeleteCorrectionResult(initial.Date)).Subscribe(observer))
                    .DisposeWith(Disposer);
        }
    }

    public abstract record CorrectionResult;

    public sealed record CancelCorrectionResult : CorrectionResult;

    public sealed record UpdateCorrectionResult(ProfileEntry Entry) : CorrectionResult;

    public sealed record DeleteCorrectionResult(DateTime Key) : CorrectionResult;
}
