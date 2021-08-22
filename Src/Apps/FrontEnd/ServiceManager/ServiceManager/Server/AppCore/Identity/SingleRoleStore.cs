using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ServiceManager.Server.AppCore.Identity
{ 
    public sealed class SingleRoleStore : IRoleStore<AdminClaim>
    {
        private AdminClaim _claim = new();

        public void Dispose()
        {
            
        }

        private Task<IdentityResult> GetModifyFail()
            => Task.FromResult(IdentityResult.Failed(new IdentityError { Code = "405", Description = "Modifikation der Rollen Verboten" }));

        public Task<IdentityResult> CreateAsync(AdminClaim role, CancellationToken cancellationToken)
            => GetModifyFail();

        public Task<IdentityResult> UpdateAsync(AdminClaim role, CancellationToken cancellationToken)
            => GetModifyFail();

        public Task<IdentityResult> DeleteAsync(AdminClaim role, CancellationToken cancellationToken)
            => GetModifyFail();

        public Task<string> GetRoleIdAsync(AdminClaim role, CancellationToken cancellationToken)
            => Task.FromResult(nameof(AdminClaim));

        public Task<string> GetRoleNameAsync(AdminClaim role, CancellationToken cancellationToken)
            => Task.FromResult(_claim.Name);

        public Task SetRoleNameAsync(AdminClaim role, string roleName, CancellationToken cancellationToken)
        {
            _claim = _claim with{Name = roleName};

            return Task.CompletedTask;
        }

        public Task<string> GetNormalizedRoleNameAsync(AdminClaim role, CancellationToken cancellationToken)
            => Task.FromResult(_claim.NormailzedName);

        public Task SetNormalizedRoleNameAsync(AdminClaim role, string normalizedName, CancellationToken cancellationToken)
        {
            _claim = _claim with { NormailzedName = normalizedName };

            return Task.CompletedTask;
        }

        public Task<AdminClaim> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            if (roleId != nameof(AdminClaim))
                throw new InvalidOperationException("Nur eine Rolle ist Verfügbar");

            return Task.FromResult(_claim);
        }

        public Task<AdminClaim> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            if (_claim.NormailzedName != normalizedRoleName)
                throw new InvalidOperationException("Keine Rolle gefunden");

            return Task.FromResult(_claim);
        }
    }
}