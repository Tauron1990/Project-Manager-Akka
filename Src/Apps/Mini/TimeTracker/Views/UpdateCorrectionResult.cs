using TimeTracker.Data;

namespace TimeTracker.Views;

public sealed record UpdateCorrectionResult(ProfileEntry Entry) : CorrectionResult;