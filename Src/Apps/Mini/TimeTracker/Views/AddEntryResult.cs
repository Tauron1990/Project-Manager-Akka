using TimeTracker.Data;

namespace TimeTracker.Views
{
    public abstract record AddEntryResult;

    public sealed record CancelAddEntryResult : AddEntryResult;

    public sealed record NewAddEntryResult(ProfileEntry Entry) : AddEntryResult;
}