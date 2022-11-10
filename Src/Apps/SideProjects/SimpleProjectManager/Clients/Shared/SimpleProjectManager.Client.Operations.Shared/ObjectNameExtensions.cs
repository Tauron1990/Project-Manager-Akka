namespace SimpleProjectManager.Client.Operations.Shared;

public static class ObjectNameExtensions
{
    public static bool IsInValid(this ObjectName? name)
        => string.IsNullOrWhiteSpace(name?.Value);
}