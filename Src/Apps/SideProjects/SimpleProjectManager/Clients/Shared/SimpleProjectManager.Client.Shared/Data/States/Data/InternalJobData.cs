using System;
using System.Linq;

namespace SimpleProjectManager.Client.Shared.Data.States.Data;

public sealed record InternalJobData(JobSortOrderPair[] CurrentJobs, CurrentSelected CurrentSelected)
{
    public InternalJobData()
        : this(Array.Empty<JobSortOrderPair>(), new CurrentSelected(null, null))
    { }

    public bool Equals(InternalJobData? other)
    {
        if (other is null) return false;

        return CurrentSelected == other.CurrentSelected && CurrentJobs.SequenceEqual(other.CurrentJobs);
    }

    public override int GetHashCode()
        => HashCode.Combine(CurrentSelected, CurrentJobs.Aggregate(0, (i, pair) => HashCode.Combine(i, pair.GetHashCode())));
}