using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Util;
using Tauron;
using Tauron.Application;
using Tauron.Application.CommonUI.Commands;
using Tauron.Application.CommonUI.Dialogs;
using Tauron.Application.CommonUI.Model;
using Tauron.Application.Wpf.Dialogs;
using TimeTracker.Data;

namespace TimeTracker.Views
{
    /// <summary>
    /// Interaktionslogik für CorrectionDialog.xaml
    /// </summary>
    public partial class CorrectionDialog : IBaseDialog<ProfileEntry?, ProfileEntry>
    {
        public CorrectionDialog()
        {
            InitializeComponent();
        }

        public Task<ProfileEntry?> Init(ProfileEntry initalData) 
            => this.MakeObsTask<ProfileEntry?>(o => new CorrectionDialogModel(o, initalData));
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
            set => SetValue(string.Empty);
        }

        public string Date
        {
            get => GetValue(string.Empty);
            set => SetValue(string.Empty);
        }

        public SimpleReactiveCommand Apply { get; }

        public SimpleReactiveCommand Cancel { get; }

        public CorrectionDialogModel(IObserver<ProfileEntry?> observer, ProfileEntry initial)
        {
            Date = initial.Date.ToString("d");
            FinishTime = (initial.Finish ?? TimeSpan.Zero).ToString(@"hh\:mm");
            StartTime = (initial.Start ?? TimeSpan.Zero).ToString(@"hh\:mm");

            AddValidation(
                () => StartTime,
                sobs => from time in sobs
                        let ty = Try<TimeSpan>.From(() => TimeSpan.Parse(time))
                        select ty.IsSuccess ? null : ty.Failure.Value.Message);
            AddValidation(
                () => FinishTime,
                sobs => from time in sobs
                        let ty = Try<TimeSpan>.From(() => TimeSpan.Parse(time))
                        select ty.IsSuccess ? null : ty.Failure.Value.Message);

            AddValidation(
                () => Date,
                sobs => from time in sobs
                        let ty = Try<DateTime>.From(() => DateTime.Parse(time))
                        select ty.IsSuccess ? null : ty.Failure.Value.Message);

            Apply = new SimpleReactiveCommand(from _ in ErrorsChanged
                                              select ErrorCount == 0)
                   .Finish(o => o.Select(_ => new ProfileEntry(DateTime.Parse(Date).Date, TimeSpan.Parse(StartTime), TimeSpan.Parse(FinishTime)))
                                 .Subscribe(observer))
                   .DisposeWith(Disposer);

            Cancel = new SimpleReactiveCommand()
                    .Finish(o => o.Select(_ => default(ProfileEntry)).Subscribe(observer))
                    .DisposeWith(Disposer);
        }
    }
}
