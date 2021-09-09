using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Authorization;

namespace ServiceManager.Client.ViewModels.Identity
{
    public interface IErrorMessageProvider
    {
        IEnumerable<string> GetMessage(AuthenticationState state, string[] roles);
    }
}