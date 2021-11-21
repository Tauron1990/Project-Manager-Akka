namespace SimpleProjectManager.Server.Core.Tasks;

public sealed record TasksChanged
{
    public static readonly TasksChanged Inst = new ();
}