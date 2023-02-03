using System;

namespace TimeTracker.Views;

public sealed record DeleteCorrectionResult(DateTime Key) : CorrectionResult;