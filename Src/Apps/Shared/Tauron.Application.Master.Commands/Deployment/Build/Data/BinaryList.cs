using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Tauron.Application.Master.Commands.Deployment.Build.Data
{
    public sealed record BinaryList(ImmutableList<AppBinary> AppBinaries) : IEnumerable<AppBinary>
    {
        public IEnumerator<AppBinary> GetEnumerator() => AppBinaries.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) AppBinaries).GetEnumerator();
        public static readonly BinaryList Empty = new(ImmutableList<AppBinary>.Empty);
    }
}