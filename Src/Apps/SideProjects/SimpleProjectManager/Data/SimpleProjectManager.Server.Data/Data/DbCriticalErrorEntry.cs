namespace SimpleProjectManager.Server.Data.Data;

public class DbCriticalErrorEntry
{
    public string Id { get; set; }
    
    public DbCriticalError Error { get; set; }
    
    public bool IsDisabled { get; set; }
}