using System;
using System.Globalization;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Tauron;
using Tauron.Application;
using Tauron.Application.CommonUI.Commands;
using Tauron.Application.CommonUI.Model;
using TimeTracker.Managers;

namespace TimeTracker.Views;

public sealed class ConfigurationDialogModel : ObservableObject, IDisposable
{
    private readonly CompositHolder _disposer = new();
    private string _dailyHours = string.Empty;
    private string _minusShortTimeHours = string.Empty;

    private string _monthHours = string.Empty;

    public ConfigurationDialogModel(IObserver<Unit> close, ConfigurationManager manager)
    {
        MonthHours = manager.MonthHours.ToString(CultureInfo.CurrentUICulture);
        MinusShortTimeHours = manager.MinusShortTimeHours.ToString(CultureInfo.CurrentUICulture);
        DailyHours = manager.DailyHours.ToString(CultureInfo.CurrentUICulture);

        Ok = new SimpleReactiveCommand()
            .Finish(
                obs => (from trigger in obs
                        let one = TryUpdate(MonthHours, i => manager.MonthHours = i)
                        let two = TryUpdate(MinusShortTimeHours, i => manager.MinusShortTimeHours = i)
                        let three = TryUpdate(DailyHours, i => manager.DailyHours = i)
                        select trigger)
                    .Subscribe(close))
            .DisposeWith(_disposer);

        (from newValue in this.WhenAny(() => MonthHours)
                let result = Try.From(() => int.Parse(newValue, CultureInfo.CurrentUICulture))
                where result.IsSuccess
                select result.Get())
            .Subscribe(
                i =>
                {
                    try
                    {
                        MinusShortTimeHours = (i * 0.10d).ToString("F0", CultureInfo.CurrentUICulture);
                        DailyHours = (i / 20d).ToString("F0", CultureInfo.CurrentUICulture);
                    }
                    catch
                    {
                        // ignored
#pragma warning disable ERP022
                    }
#pragma warning restore ERP022
                })
            .DisposeWith(_disposer);
    }

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

    public void Dispose() => _disposer.Dispose();

    private bool TryUpdate(string value, Action<int> setter)
    {
        if (!int.TryParse(value, CultureInfo.CurrentUICulture, out var i) || i <= 0) return false;

        setter(i);

        return true;
    }
}