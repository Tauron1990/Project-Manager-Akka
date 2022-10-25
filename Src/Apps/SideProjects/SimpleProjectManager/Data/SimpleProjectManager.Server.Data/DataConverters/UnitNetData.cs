namespace SimpleProjectManager.Server.Data.DataConverters;

public record UnitNetData
{
    public string Type { get; init; }

    public double Value { get; init; }
}