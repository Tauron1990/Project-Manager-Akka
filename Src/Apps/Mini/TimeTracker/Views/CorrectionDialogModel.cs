using System;
using System.Globalization;
using System.Reactive.Linq;
using System.Threading;
using Tauron;
using Tauron.Application.CommonUI.Commands;
using Tauron.Application.CommonUI.Model;
using TimeTracker.Data;
using TimeTracker.Managers;

namespace TimeTracker.Views;

public sealed class CorrectionDialogModel : ObservableErrorObject
{
    public CorrectionDialogModel(IObserver<CorrectionResult> observer, ProfileEntry initial, HolidayManager manager)
    {
        Date = initial.Date.ToString("d", CultureInfo.CurrentUICulture);
        FinishTime = initial.Finish?.ToString(@"hh\:mm", CultureInfo.CurrentUICulture) ?? string.Empty;
        StartTime = initial.Start?.ToString(@"hh\:mm", CultureInfo.CurrentUICulture) ?? string.Empty;

        AddValidation(
            () => StartTime,
            sobs => from time in sobs
                let ty = TimeSpan.TryParse((string?)time, CultureInfo.CurrentUICulture, out _)
                select ty ? null : "Kann nicht in Zeitpunkt Umgewandelt werden");
        AddValidation(
            () => FinishTime,
            sobs => from time in sobs
                let ty = string.IsNullOrWhiteSpace(time) || TimeSpan.TryParse(time, CultureInfo.CurrentUICulture, out _)
                select ty ? null : "Kann nicht in Zeitpunkt Umgewandelt werden");

        AddValidation(
            () => Date,
            sobs => from time in sobs
                let ty = DateTime.TryParse(time,CultureInfo.CurrentUICulture, out _)
                select ty ? null : "Kann nicht in Datum Umgewandelt werden");

        Apply = new SimpleReactiveCommand(
                (from _ in ErrorsChanged
                    select ErrorCount == 0).StartWith(ErrorCount == 0))
            .Finish(
                obs => (from _ in obs
                        let date = DateTime.Parse(Date, CultureInfo.CurrentUICulture).Date
                        let start = TimeSpan.Parse(StartTime, CultureInfo.CurrentUICulture)
                        let end = string.IsNullOrWhiteSpace(FinishTime) ? Timeout.InfiniteTimeSpan : TimeSpan.Parse(FinishTime, CultureInfo.CurrentUICulture)
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
}