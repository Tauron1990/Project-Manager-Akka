namespace ServiceManager.Shared.Identity
{
    public sealed record UserData(string Id, string Name, int Claims);

    public sealed record UserClaim(int ClaimId, string UserId, string Name);
}