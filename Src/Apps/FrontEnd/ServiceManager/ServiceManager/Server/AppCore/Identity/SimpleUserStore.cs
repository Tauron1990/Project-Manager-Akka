using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ServiceManager.Server.AppCore.Identity
{
    public sealed class SimpleUserStore : IUserStore<SimpleUser>
    {


        public void Dispose() { }

        public Task<string> GetUserIdAsync(SimpleUser user, CancellationToken cancellationToken)
            => throw new System.NotImplementedException();

        public Task<string> GetUserNameAsync(SimpleUser user, CancellationToken cancellationToken)
            => throw new System.NotImplementedException();

        public Task SetUserNameAsync(SimpleUser user, string userName, CancellationToken cancellationToken)
            => throw new System.NotImplementedException();

        public Task<string> GetNormalizedUserNameAsync(SimpleUser user, CancellationToken cancellationToken)
            => throw new System.NotImplementedException();

        public Task SetNormalizedUserNameAsync(SimpleUser user, string normalizedName, CancellationToken cancellationToken)
            => throw new System.NotImplementedException();

        public Task<IdentityResult> CreateAsync(SimpleUser user, CancellationToken cancellationToken)
            => throw new System.NotImplementedException();

        public Task<IdentityResult> UpdateAsync(SimpleUser user, CancellationToken cancellationToken)
            => throw new System.NotImplementedException();

        public Task<IdentityResult> DeleteAsync(SimpleUser user, CancellationToken cancellationToken)
            => throw new System.NotImplementedException();

        public Task<SimpleUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
            => throw new System.NotImplementedException();

        public Task<SimpleUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
            => throw new System.NotImplementedException();
    }
}