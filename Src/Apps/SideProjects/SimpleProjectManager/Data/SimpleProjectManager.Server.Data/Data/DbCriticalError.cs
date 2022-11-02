using System.Collections.Immutable;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Server.Data.Data;

public class DbCriticalError
{
    public string Id { get; set; }
    
    public DateTime Occurrence { get; set; }
    
    public string ApplicationPart { get; set; }
    
    public string Message { get; set; }
    
    public string? StackTrace { get; set; }

    public List<DbErrorProperty> ContextData { get; set; } = new();
}