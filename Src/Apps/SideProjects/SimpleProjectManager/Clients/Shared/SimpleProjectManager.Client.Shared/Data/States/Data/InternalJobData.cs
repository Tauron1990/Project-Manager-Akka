﻿using System;
using System.Linq;

namespace SimpleProjectManager.Client.Shared.Data.States.Data;

public sealed record InternalJobData(bool IsLoaded, JobSortOrderPair[] CurrentJobs, CurrentSelected? CurrentSelected)
{
    public InternalJobData()
        : this(false, Array.Empty<JobSortOrderPair>(), new CurrentSelected(null, null)) { }

    public bool Equals(InternalJobData? other)
    {
        if(other is null) return false;

        return CurrentSelected == other.CurrentSelected && CurrentJobs.SequenceEqual(other.CurrentJobs);
    }

    public JobSortOrderPair? FindPair(string? id)
        => string.IsNullOrWhiteSpace(id) ? null : CurrentJobs.FirstOrDefault(p => p.Order.Id.Value == id);

    public override int GetHashCode()
        => HashCode.Combine(CurrentSelected, CurrentJobs.Aggregate(0, (i, pair) => HashCode.Combine(i, pair.GetHashCode())));
}