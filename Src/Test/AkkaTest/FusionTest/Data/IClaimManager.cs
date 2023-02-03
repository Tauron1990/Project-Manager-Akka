using System.Threading;
using System.Threading.Tasks;
using Stl.CommandR.Configuration;
using Stl.Fusion;

namespace AkkaTest.FusionTest.Data
{
    public interface IClaimManager
    {
        [ComputeMethod]
        Task<Claim> Get(ClaimId id);

        [ComputeMethod]
        Task<ClaimId[]> GetAll();

        [ComputeMethod]
        Task<ClaimId> GetId(string name);
        
        [CommandHandler]
        Task<ClaimId> AddClaim(AddClaimCommand command, CancellationToken token = default);
        
        [CommandHandler]
        Task RemoveClaim(RemoveClaimCommand command, CancellationToken token = default);

        [CommandHandler]
        Task UpdateClaim(UpdateClaimCommand command, CancellationToken token = default);
    }
}