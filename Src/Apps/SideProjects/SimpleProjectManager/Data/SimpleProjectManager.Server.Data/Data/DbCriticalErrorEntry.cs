using System.Diagnostics.CodeAnalysis;

namespace SimpleProjectManager.Server.Data.Data;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class DbCriticalErrorEntry
{
    public string Id { get; set; } = string.Empty;

    public DbCriticalError Error { get; set; } = new();

    public bool IsDisabled { get; set; }
}