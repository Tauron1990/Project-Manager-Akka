using System;
using System.Collections.Immutable;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace TestApp
{
    static class Program
    {
        private static (ImmutableList<JobInfo> TestData, ImmutableList<SortOrder> TestSort) GenerateTestData()
        {
            (string Name, bool Prio, int Pos, ProjectStatus Status, DateTimeOffset Deadline)[] data =
            {

            };
        }

        static void Main()
        {
        }
    }
}