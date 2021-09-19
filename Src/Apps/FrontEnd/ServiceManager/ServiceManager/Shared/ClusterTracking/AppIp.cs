namespace ServiceManager.Shared.ClusterTracking
{
    public sealed record AppIp(string Ip, bool IsValid)
    {
        public static readonly AppIp Invalid = new("Unbekannt", IsValid: false);
    }
}