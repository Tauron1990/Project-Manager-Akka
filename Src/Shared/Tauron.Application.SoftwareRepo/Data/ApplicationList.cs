using System.Collections.Immutable;

namespace Tauron.Application.SoftwareRepo.Data
{
    public sealed record ApplicationList(string Name, string Description, ImmutableList<ApplicationEntry> ApplicationEntries);
}