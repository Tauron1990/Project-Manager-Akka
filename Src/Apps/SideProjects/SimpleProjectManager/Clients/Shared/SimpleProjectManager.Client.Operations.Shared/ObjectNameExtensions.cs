namespace SimpleProjectManager.Client.Operations.Shared;

public static class ObjectNameExtensions
{
    public static bool IsInValid(this in ObjectName? name)
        => string.IsNullOrWhiteSpace(name?.Value);
}