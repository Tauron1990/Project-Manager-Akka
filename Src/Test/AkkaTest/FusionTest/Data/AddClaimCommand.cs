using System;
using Stl.CommandR;

namespace AkkaTest.FusionTest.Data
{
    public record AddClaimCommand(string Name, string Info) : ICommand<Guid>;

    public record RemoveClaimCommand(Guid Id) : ICommand;
}