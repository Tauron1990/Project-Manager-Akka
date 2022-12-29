using SimpleProjectManager.Client.Shared.Data.States.Data;

namespace SimpleProjectManager.Client.Shared.Data.States.Actions;

internal static class JobDataSelectors
{
    internal static CurrentSelected CurrentSelected(InternalJobData data)
        => data.CurrentSelected ?? new CurrentSelected(null, null);
}