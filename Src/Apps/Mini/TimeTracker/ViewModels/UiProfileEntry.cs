using System;
using System.Globalization;
using Tauron;
using Tauron.Application;
using TimeTracker.Data;
using TimeTracker.Managers;

namespace TimeTracker.ViewModels;

#pragma warning disable MA0097
public sealed class UiProfileEntry : ObservableObject, IComparable<UiProfileEntry>, IDisposable, IEquatable<UiProfileEntry>
#pragma warning restore MA0097
{
    private const string TimespanTemplate = @"hh\:mm";
    private readonly IDisposable _subscription;

    private string? _hour;

    public UiProfileEntry(ProfileEntry entry, IObservable<ProfileData> data, Action<Exception> error)
    {
        Entry = entry;
        UpdateLabels();
        _subscription = CalculationManager.GetEntryHourCalculator(entry, data)
            .AutoSubscribe(ts => Hour = ts.ToString(TimespanTemplate, CultureInfo.CurrentUICulture), error);
    }

    public ProfileEntry Entry { get; }

    public string? Date { get; private set; }

    public string? Start { get; private set; }

    public string? Finish { get; private set; }

    public string? Hour
    {
        get => _hour;
        private set => SetProperty(ref _hour, value);
    }

    public bool IsValid { get; private set; }

    public int CompareTo(UiProfileEntry? other)
        => other is null ? 1 : Entry.Date.CompareTo(other.Entry.Date) * -1;

    public void Dispose() => _subscription.Dispose();

    //public UiProfileEntry()
    //{
    //    Entry = new ProfileEntry(DateTime.Now.Date, TimeSpan.FromHours(8), TimeSpan.FromHours(16));
    //    UpdateLabels();
    //}

    private void UpdateLabels()
    {
        Date = Entry.Date.ToLocalTime().ToString("D", CultureInfo.CurrentUICulture);
        Start = Entry.Start?.ToString(TimespanTemplate, CultureInfo.CurrentUICulture);
        Finish = Entry.Finish?.ToString(TimespanTemplate, CultureInfo.CurrentUICulture);

        if (Entry.Start is null)
            Start = Entry.DayType switch
            {
                DayType.Vacation => "Urlaub",
                DayType.Holiday => "Feiertag",
                _ => string.Empty
            };

        //Hour = (Entry.Finish - Entry.Start)?.ToString(TimespanTemplate);

        var start = Entry.Start;
        var finish = Entry.Finish;
        if (Entry.DayType != DayType.Normal)
            IsValid = true;
        else if (start != null && finish is null)
            IsValid = false;
        else if (start > finish)
            IsValid = false;
        else
            IsValid = true;
    }

    public bool Equals(UiProfileEntry? other)
    {
        if(ReferenceEquals(null, other))
            return false;
        if(ReferenceEquals(this, other))
            return true;

        return Entry.Date.Equals(other.Entry.Date);
    }

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is UiProfileEntry other && Equals(other);

    public override int GetHashCode() => Entry.Date.GetHashCode();

    public static bool operator ==(UiProfileEntry? left, UiProfileEntry? right) => Equals(left, right);

    public static bool operator !=(UiProfileEntry? left, UiProfileEntry? right) => !Equals(left, right);
}