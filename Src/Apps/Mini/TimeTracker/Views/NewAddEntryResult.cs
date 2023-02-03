using TimeTracker.Data;

namespace TimeTracker.Views;

public sealed record NewAddEntryResult(ProfileEntry Entry) : AddEntryResult;