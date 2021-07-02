namespace Tauron.Application.ServiceManager.AppCore
{
    public sealed record AppIp(string Ip, bool IsValid)
    {
        public static readonly AppIp Invalid = new("Unbekannt", false);
    }
}