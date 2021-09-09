using System.Collections.Immutable;
using Stl.CommandR;
using Stl.Fusion.Authentication;

namespace ServiceManager.Shared.Identity
{
    public sealed record StartSetupCommand(string AdminName, string AdminPassword) : ICommand<string>;

    public sealed record TryLoginCommand(string UserName, string Password, Session Session) : ICommand<string>;

    public sealed record RegisterUserCommand(string UserName, string Password) : ICommand<string>;

    public sealed record SetClaimsCommand(string UserId, ImmutableList<string> Claims) : ICommand<string>;

    public sealed record SetNewPasswordCommand(string UserId, string OldPassword, string NewPassword) : ICommand<string>;

    public sealed record DeleteUserCommand(string UserId) : ICommand<string>;

    public sealed record LogOutCommand(Session Session) : ICommand<string>;
}