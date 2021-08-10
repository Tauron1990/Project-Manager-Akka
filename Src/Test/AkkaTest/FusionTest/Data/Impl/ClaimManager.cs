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

        public virtual Task<Claim> Get(Guid id)
            => Task.FromResult(_claims.TryGetValue(id, out var claim) ? claim : Claim.Invalid);

        public virtual Task<Guid[]> GetAll()
            => Task.FromResult(_claims.Values.Select(c => c.Id).ToArray());

        public virtual Task<Guid> GetId(string name)
            => Task.FromResult((_claims.Values.FirstOrDefault(c => c.Name == name) ?? Claim.Invalid).Id);

        public virtual Task<Guid> AddClaim(AddClaimCommand command, CancellationToken token = default)
        {
            if (_claims.Any(c => c.Value.Name == command.Name))
                return Task.FromResult(Claim.Invalid.Id);

            var id    = Deterministic.Create(Namespace, command.Name);
            var    claim = new Claim(id, command.Name, command.Info, DateTime.Now);

            if (!_claims.TryAdd(id, claim)) 
                return Task.FromException<Guid>(new InvalidOperationException("Duplicate Id Found"));

            using (Computed.Invalidate())
            {
                Get(id).Ignore();
                GetAll().Ignore();
                GetId(command.Name).Ignore();
            }
                
            return Task.FromResult(id);

        }

        public virtual Task RemoveClaim(RemoveClaimCommand command, CancellationToken token = default)
        {
            if(!_claims.TryRemove(command.Id, out var claim))
                return Task.CompletedTask;

            using (Computed.Invalidate())
            {
                Get(claim.Id).Ignore();
                GetAll().Ignore();
                GetId(claim.Name).Ignore();
            }
            
            return Task.CompletedTask;
        }

        public Task UpdateClaim(UpdateClaimCommand command, CancellationToken token = default)
        {
            if (!_claims.ContainsKey(command.Data.Id))
                throw new InvalidOperationException("Claim does not Exist");
            _claims[command.Data.Id] = command.Data;

            using (Computed.Invalidate()) 
                Get(command.Data.Id).Ignore();

            return Task.CompletedTask;
        }
    }
}