using System.Diagnostics.CodeAnalysis;

namespace SimpleProjectManager.Server.Data.Data;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class DbCriticalError
{
    public string Id { get; set; } = string.Empty;

    public DateTime Occurrence { get; set; }

    public string ApplicationPart { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? StackTrace { get; set; }

    public List<DbErrorProperty> ContextData { get; set; } = new();
}