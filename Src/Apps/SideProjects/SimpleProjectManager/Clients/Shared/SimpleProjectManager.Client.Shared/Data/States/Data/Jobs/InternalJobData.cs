using System;
using System.Linq;
using SimpleProjectManager.Client.Shared.Data.States.Actions;

namespace SimpleProjectManager.Client.Shared.Data.States.Data;

public sealed record InternalJobData(bool IsLoaded, JobSortOrderPair[] CurrentJobs, CurrentSelected? CurrentSelected)
{
    public InternalJobData()
        : this(IsLoaded: false, Array.Empty<JobSortOrderPair>(), new CurrentSelected(Pair: null, JobData: null)) { }

    public bool Equals(InternalJobData? other)
    {
        if(other is null) return false;

        return CurrentSelected == other.CurrentSelected && CurrentJobs.SequenceEqual(other.CurrentJobs);
    }

    #pragma warning disable EPS05
    public JobSortOrderPair? FindPair(PairSelection id)
#pragma warning restore EPS05
        => id == PairSelection.Nothing
            ? null
            : CurrentJobs.FirstOrDefault(
                p => p.Order.Id?.Value == id);

    public override int GetHashCode()
        => HashCode.Combine(CurrentSelected, CurrentJobs.Aggregate(0, (i, pair) => HashCode.Combine(i, pair.GetHashCode())));
}