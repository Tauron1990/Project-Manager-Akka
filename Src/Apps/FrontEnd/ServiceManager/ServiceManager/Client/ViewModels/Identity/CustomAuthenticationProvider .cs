using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Stl.Fusion.Blazor;

namespace ServiceManager.Client.ViewModels.Identity
{
    public sealed class CustomAuthenticationProvider: AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult<AuthenticationState>(new UserState(new UserData("Gast", ImmutableDictionary<string, string>.Empty, true)));
    }
}