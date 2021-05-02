namespace ServiceHost.ApplicationRegistry
{
    public sealed record AllAppsQuery;

    public sealed record AllAppsResponse(string[] Apps);
}