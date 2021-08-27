using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ServiceManager.Shared.Identity;
using Stl.Fusion;
using Stl.Fusion.EntityFramework.Authentication;

namespace ServiceManager.Server.AppCore.Identity
{
    public class UserManagment : Stl.Fusion.EntityFramework.DbServiceBase<UsersDatabase>, IUserManagement
    {
        private readonly IDbUserRepo<UsersDatabase, FusionUserEntity, long> _repo;
        public UserManagment(IServiceProvider services, IDbUserRepo<UsersDatabase, FusionUserEntity, long> repo) 
            : base(services) => _repo = repo;


        public virtual async Task<bool> NeedSetup(CancellationToken token = default)
        {
            if (Computed.IsInvalidating()) return false;

            await using var db = CreateDbContext();

            return await db.FusionUsers.CountAsync(token) == 0;
        }
    }
}