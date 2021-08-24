using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace ServiceManager.Client.ViewModels.Identity
{
    public sealed class UserState : AuthenticationState
    {
        public UserData ActualUser { get; }

        public bool IsAnonymos => ActualUser.IsAnonymos; 

        public UserState(UserData actualUser) : base(new ClaimsPrincipal(new ClaimsIdentity(actualUser.Claims.Select(p => new Claim(p.Key, p.Value)))))
            => ActualUser = actualUser;
    }
}