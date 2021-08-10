using System;
using System.Threading;
using System.Threading.Tasks;
using Stl.CommandR.Configuration;
using Stl.Fusion;

namespace AkkaTest.FusionTest.Data
{
    public interface IClaimManager
    {
        [ComputeMethod]
        Task<Claim> Get(Guid id);

        [ComputeMethod]
        Task<Guid[]> GetAll();

        [ComputeMethod]
        Task<Guid> GetId(string name);
        
        [CommandHandler]
        Task<Guid> AddClaim(AddClaimCommand command, CancellationToken token = default);
        
        [CommandHandler]
        Task RemoveClaim(RemoveClaimCommand command, CancellationToken token = default);

        [CommandHandler]
        Task UpdateClaim(UpdateClaimCommand command, CancellationToken token = default);
    }
}