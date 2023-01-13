namespace SimpleProjectManager.Server.Data.DataConverters;

public record UnitNetData
{
    public string Type { get; init; } = string.Empty;

    public double Value { get; init; }
}