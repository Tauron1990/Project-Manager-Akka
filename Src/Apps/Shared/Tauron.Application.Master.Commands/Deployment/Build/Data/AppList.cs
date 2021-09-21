using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Tauron.Application.Master.Commands.Deployment.Build.Data
{
    public sealed record AppList(ImmutableList<AppInfo> Apps) : IEnumerable<AppInfo>
    {
        public static readonly AppList Empty = new(ImmutableList<AppInfo>.Empty);
        public IEnumerator<AppInfo> GetEnumerator() => Apps.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Apps).GetEnumerator();
    }
}