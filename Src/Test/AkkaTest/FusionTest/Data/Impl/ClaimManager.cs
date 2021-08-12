using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Generators.Guid;
using Stl.Async;
using Stl.Fusion;

namespace AkkaTest.FusionTest.Data.Impl
{
    public class ClaimManager : IClaimManager
    {
        private static readonly Guid Namespace = Guid.Parse("E370A3AF-D2CC-4319-B345-9747DED3F510");
        
        private readonly ConcurrentDictionary<Guid, Claim> _claims = new();

        public virtual Task<Claim> Get(ClaimId id)
            => Task.FromResult(_claims.TryGetValue(id.Data, out var claim) ? claim : Claim.Invalid);

        public virtual Task<ClaimId[]> GetAll()
            => Task.FromResult(_claims.Values.Select(c => c.Id).ToArray());

        public virtual Task<ClaimId> GetId(string name)
            => Task.FromResult((_claims.Values.FirstOrDefault(c => c.Name == name) ?? Claim.Invalid).Id);

        public virtual Task<ClaimId> AddClaim(AddClaimCommand command, CancellationToken token = default)
        {
            if (_claims.Any(c => c.Value.Name == command.Name))
                return Task.FromResult(Claim.Invalid.Id);

            var id    = Deterministic.Create(Namespace, command.Name);
            var    claim = new Claim(new ClaimId(id), command.Name, command.Info, DateTime.Now);

            if (!_claims.TryAdd(id, claim)) 
                return Task.FromException<ClaimId>(new InvalidOperationException("Duplicate Id Found"));

            using (Computed.Invalidate())
            {
                Get(new ClaimId(id)).Ignore();
                GetAll().Ignore();
                GetId(command.Name).Ignore();
            }
                
            return Task.FromResult(new ClaimId(id));
        }

        public virtual Task RemoveClaim(RemoveClaimCommand command, CancellationToken token = default)
        {
            if(!_claims.TryRemove(command.Id.Data, out var claim))
                return Task.CompletedTask;

            using (Computed.Invalidate())
            {
                Get(claim.Id).Ignore();
                GetAll().Ignore();
                GetId(claim.Name).Ignore();
            }
            
            return Task.CompletedTask;
        }

        public virtual Task UpdateClaim(UpdateClaimCommand command, CancellationToken token = default)
        {
            if (!_claims.ContainsKey(command.Id.Data))
                throw new InvalidOperationException("Claim does not Exist");
            _claims[command.Id.Data] = _claims[command.Id.Data] with{ Info = command.Info };

            using (Computed.Invalidate()) 
                Get(command.Id).Ignore();

            return Task.CompletedTask;
        }
    }
}