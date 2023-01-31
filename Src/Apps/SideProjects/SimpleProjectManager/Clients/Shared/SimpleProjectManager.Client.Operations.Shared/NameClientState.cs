namespace SimpleProjectManager.Client.Operations.Shared;

public enum NameClientState
{
    UnInitialized,
    Offline,
    Failed,
    Online,
    Duplicate,
    TimeOut,
    InConsistetent,
}