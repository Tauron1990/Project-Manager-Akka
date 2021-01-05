using System.Collections.Generic;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.Analyzing
{
    [PublicAPI]
    public sealed record IssuesEvent(string RuleName, IEnumerable<Issue> Issues);
}