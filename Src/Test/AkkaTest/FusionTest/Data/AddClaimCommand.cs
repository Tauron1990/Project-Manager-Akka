using System;
using System.Reactive;
using Stl.CommandR;

namespace AkkaTest.FusionTest.Data
{
    public record AddClaimCommand(string Name, string Info) : ICommand<Guid>;

    public record RemoveClaimCommand(Guid Id) : ICommand<Unit>;

    public record UpdateClaimCommand(Claim Data) : ICommand<Unit>;
}