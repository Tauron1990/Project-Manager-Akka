using Stl.CommandR;
using Stl.Fusion.Authentication;

namespace ServiceManager.Shared.Identity
{
    public sealed record StartSetupCommand(string AdminName, string AdminPassword) : ICommand<string>;

    public sealed record TryLoginCommand(string UserName, string Password, Session Session);

    public sealed record RegisterUserCommand(string UserName, string Password);
}