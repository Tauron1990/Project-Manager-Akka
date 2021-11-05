using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace TestApp
{
    static class Program
    {
        public sealed record JobSortOrderPair(SortOrder Order, JobInfo Info);
        private static (ImmutableList<JobInfo> TestData, ImmutableList<SortOrder> TestSort) GenerateTestData()
        {
            var now = DateTime.Now;

            (string Name, bool Prio, int Pos, ProjectStatus Status, DateTime? Deadline)[] data =
            {
                ("T1_10000", false, 0, ProjectStatus.Entered, now + TimeSpan.FromDays(1)),
                ("T1_20000", false, 0, ProjectStatus.Entered, now + TimeSpan.FromDays(2)),
                ("T1_40000", false, 0, ProjectStatus.Entered, now + TimeSpan.FromDays(3)),
                ("T1_50000", false, 0, ProjectStatus.Entered, now + TimeSpan.FromDays(4)),
                ("T1_30000", false, 2, ProjectStatus.Entered, now + TimeSpan.FromDays(5)),

                ("T2_10000", true, 0, ProjectStatus.Pending, now + TimeSpan.FromDays(1)),
                ("T2_20000", true, 0, ProjectStatus.Pending, now + TimeSpan.FromDays(2)),
                ("T2_40000", true, 0, ProjectStatus.Pending, now + TimeSpan.FromDays(3)),
                ("T2_50000", true, 0, ProjectStatus.Pending, now + TimeSpan.FromDays(4)),
                ("T2_30000", true, 2, ProjectStatus.Pending, now + TimeSpan.FromDays(5)),

                ("T3_10000", false, 0, ProjectStatus.Finished, now + TimeSpan.FromDays(1)),
                ("T3_20000", false, 0, ProjectStatus.Finished, now + TimeSpan.FromDays(2)),
                ("T3_40000", false, 0, ProjectStatus.Finished, now + TimeSpan.FromDays(3)),
                ("T3_50000", false, 0, ProjectStatus.Finished, now + TimeSpan.FromDays(4)),
                ("T3_30000", false, 2, ProjectStatus.Finished, now + TimeSpan.FromDays(5)),

                ("T4_10000", false, 0, ProjectStatus.Entered, null),
                ("T4_40000", false, 0, ProjectStatus.Entered, null),
                ("T4_30000", false, 0, ProjectStatus.Entered, null),
                ("T4_20000", false, 2, ProjectStatus.Entered, null),
                ("T4_50000", false, 0, ProjectStatus.Entered, null),
            };

            var jobs = ImmutableList<JobInfo>.Empty;
            var sorts = ImmutableList<SortOrder>.Empty;

            foreach (var tuple in data)
            {
                var name = new ProjectName(tuple.Name);
                var id = ProjectId.For(name);
                
                jobs = jobs.Add(new JobInfo(id, name, ProjectDeadline.FromDateTime(tuple.Deadline), tuple.Status, false));
                sorts = sorts.Add(new SortOrder(id, tuple.Pos, tuple.Prio));
            }

            return (jobs, sorts);
        }

        static void Main()
        {
            var data = GenerateTestData();

            var result = ComputeState(data.TestData, data.TestSort);

            foreach (var (sortOrder, (_, name, deadline, status, _)) in result) 
                Console.WriteLine(string.Join(", ", name.Value, status, sortOrder.IsPriority, deadline?.Value.ToString("d") ?? "null"));
        }

        public static ImmutableList<JobSortOrderPair> ComputeState(ImmutableList<JobInfo> CurrentJobs, ImmutableList<SortOrder> Sorts)
        {
            try
            {
                var list = new List<JobSortOrderPair>();

                // ReSharper disable once InvertIf
                if (CurrentJobs != null)
                {
                    var activeJobs = CurrentJobs.ToDictionary(c => c.Project);
                    foreach (var sortOrder in Sorts)
                    {
                        if (activeJobs.Remove(sortOrder.Id, out var data))
                            list.Add(new JobSortOrderPair(sortOrder, data));
                    }

                    list.AddRange(activeJobs.Select(job => new JobSortOrderPair(new SortOrder(job.Key, 0, false), job.Value)));
                }

                return list
                    //.OrderByDescending(j => j, JobSortOrderPairComparer.Comp)
                    .GroupBy(p => p.Info.Status)
                    .OrderByDescending(g => g.Key)
                    .SelectMany(StatusToPrioritySort)
                    .ToImmutableList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static IEnumerable<JobSortOrderPair> StatusToPrioritySort(IGrouping<ProjectStatus, JobSortOrderPair> statusGroup)
        {
            if (statusGroup.Key is ProjectStatus.Entered or ProjectStatus.Pending)
            {
                return statusGroup.GroupBy(s => s.Order.IsPriority)
                    .OrderByDescending(g1 => g1.Key)
                    .SelectMany(PriorityToSkipCountSort);
            }

            return statusGroup.OrderBy(p => p.Info.Deadline?.Value ?? DateTimeOffset.MaxValue);
        }

        private static IEnumerable<JobSortOrderPair> PriorityToSkipCountSort(IGrouping<bool, JobSortOrderPair> priorityGroup)
        {
            var temp = priorityGroup.OrderBy(p => p.Info.Deadline?.Value ?? DateTimeOffset.MaxValue).ToArray();
            for (var index = 0; index < temp.ToArray().Length; index++)
            {
                var orderPair = temp.ToArray()[index];

                if (orderPair.Order.SkipCount == 0)
                    continue;

                SwapArray(orderPair, index, temp);
            }

            return temp;
        }

        private static void SwapArray(JobSortOrderPair orderPair, int index, JobSortOrderPair[] temp)
        {
            for (var i = 0; i < orderPair.Order.SkipCount; i++)
            {
                if (index - i - 1 >= temp.Length) break;

                temp[index - i] = temp[index - i - 1];
            }

            if (index - orderPair.Order.SkipCount >= temp.Length)
                temp[^1] = orderPair;
            else
                temp[index - orderPair.Order.SkipCount] = orderPair;
        }
    }
}