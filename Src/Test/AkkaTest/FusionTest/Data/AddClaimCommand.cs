using System.Reactive;
using Stl.CommandR;

namespace AkkaTest.FusionTest.Data
{
    public record AddClaimCommand(string Name, string Info) : ICommand<ClaimId>;

    public record RemoveClaimCommand(ClaimId Id) : ICommand<Unit>;

    public record UpdateClaimCommand(ClaimId Id, string Info) : ICommand<Unit>;
}