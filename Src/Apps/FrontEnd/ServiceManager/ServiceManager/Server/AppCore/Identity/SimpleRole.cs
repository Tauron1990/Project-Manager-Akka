using Microsoft.AspNetCore.Identity;

namespace ServiceManager.Server.AppCore.Identity
{
    public sealed class SimpleRole : IdentityRole { }
    
    public sealed class SimpleRoleClaim :IdentityRoleClaim<string>{ }
}