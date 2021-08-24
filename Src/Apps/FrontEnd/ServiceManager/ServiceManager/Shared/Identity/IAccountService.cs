namespace ServiceManager.Shared.Identity
{
    public interface IAccountService
    {
        bool Login();
        bool Logout();
    }
}