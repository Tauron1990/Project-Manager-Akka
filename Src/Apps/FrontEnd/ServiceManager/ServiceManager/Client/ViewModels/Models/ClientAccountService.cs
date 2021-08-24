using ServiceManager.Shared.Identity;

namespace ServiceManager.Client.ViewModels.Models
{
    public class ClientAccountService : IAccountService
    {
        public bool Login()
            => false;

        public bool Logout()
            => false;
    }
}