using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Shared.Data.States;

/*public sealed class JobSortOrderPairComparer : IComparer<JobSortOrderPair>
{
    public static readonly JobSortOrderPairComparer Comp = new();

    public int Compare(JobSortOrderPair? x, JobSortOrderPair? y)
    {
        static bool StatusMatch(ProjectStatus status)
            => status is ProjectStatus.Entered or ProjectStatus.Pending;

        static int CompareDate(DateTimeOffset? x, DateTimeOffset? y)
        {
            if (x is not null) return y is null ? 1 : x.Value.CompareTo(y.Value);
            if (y is null) return 0;

            return -1;
        }


        if (x is null)
        {
            if (y is null) return 0;

            return -1;
        }

        if (y is null) return 1;

        if (!StatusMatch(x.Info.Status)) return x.Info.Status.CompareTo(y.Info.Status);
        
        if (!StatusMatch(y.Info.Status)) return 1;
        
        if (x.Order.IsPriority)
            return y.Order.IsPriority ? CompareDate(x.Info.Deadline?.Value, y.Info.Deadline?.Value) : 1;

        if (y.Order.IsPriority)
            return -1;

        return CompareDate(x.Info.Deadline?.Value, y.Info.Deadline?.Value);

    }
}*/

public sealed record JobSortOrderPair(SortOrder Order, JobInfo Info);