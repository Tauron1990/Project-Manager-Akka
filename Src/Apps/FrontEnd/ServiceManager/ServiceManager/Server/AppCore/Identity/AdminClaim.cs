namespace ServiceManager.Server.AppCore.Identity
{
    public record AdminClaim(string Name = nameof(AdminClaim), string NormailzedName = nameof(AdminClaim));
}