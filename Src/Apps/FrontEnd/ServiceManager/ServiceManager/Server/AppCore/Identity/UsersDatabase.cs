using Hyperion.Internal;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Stl.Fusion.EntityFramework.Authentication;
using Stl.Fusion.EntityFramework.Extensions;
using Stl.Fusion.EntityFramework.Operations;

namespace ServiceManager.Server.AppCore.Identity
{
    public sealed class FusionUserEntity : DbUser<string> { }

    public class FusionUserIdentityEntity : DbUserIdentity<string> { }

    public class FusionSessionInfoEntity : DbSessionInfo<string> { }

    [UsedImplicitly]
    public sealed class UsersDatabseContextBuilder : IDesignTimeDbContextFactory<UsersDatabase>
    {
        public UsersDatabase CreateDbContext(string[] args)
        {
            var ops = new DbContextOptionsBuilder<UsersDatabase>();
            ops.UseSqlite("Data Source=ServiceManager.Server.db");

            return new UsersDatabase(ops.Options);
        }
    }

    [UsedImplicitly]
    public sealed class UsersDatabase : IdentityDbContext
    {
        public UsersDatabase(DbContextOptions<UsersDatabase> options)
            : base(options) { }

        // Stl.Fusion.EntityFramework tables
        public DbSet<FusionUserEntity> FusionUsers => Set<FusionUserEntity>();
        public DbSet<FusionUserIdentityEntity> FusionUserIdentities => Set<FusionUserIdentityEntity>();
        public DbSet<FusionSessionInfoEntity> FusionSessions => Set<FusionSessionInfoEntity>();
        public DbSet<DbKeyValue> KeyValues => Set<DbKeyValue>();
        public DbSet<DbOperation> Operations => Set<DbOperation>();
    }
}