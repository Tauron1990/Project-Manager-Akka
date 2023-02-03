using TimeTracker.Data;

namespace TimeTracker.Managers;

public sealed record CalculationResult(MonthState MonthState, int Remaining);