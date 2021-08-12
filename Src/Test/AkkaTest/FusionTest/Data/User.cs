using System;

namespace AkkaTest.FusionTest.Data
{
    public sealed record User(Guid Id, string Name, string Info, DateTime CreationTime, DateTime ModifyTime)
    {
        public static readonly User Invalid = new(Guid.Empty, string.Empty, string.Empty, DateTime.MinValue, DateTime.MinValue);
    }

    public sealed record UserClaim(Guid Id, Guid UserId, Guid ClaimId);

    public sealed record Claim(ClaimId Id, string Name, string Info, DateTime CreationTime)
    {
        public static readonly Claim Invalid = new(new ClaimId(Guid.Empty), string.Empty, string.Empty, DateTime.MinValue);
    }
}