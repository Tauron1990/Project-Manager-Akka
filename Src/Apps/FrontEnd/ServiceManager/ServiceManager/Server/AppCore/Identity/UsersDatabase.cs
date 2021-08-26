using Hyperion.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Stl.Fusion.EntityFramework.Authentication;
using Stl.Fusion.EntityFramework.Extensions;
using Stl.Fusion.EntityFramework.Operations;

namespace ServiceManager.Server.AppCore.Identity
{
    public sealed class UserEntity: DbUser<long>
    {

    }

    public class UserIdentityEntity : DbUserIdentity<long>
    {
        
    }

    public class SessionInfoEntity : DbSessionInfo<long>
    {

    }

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
    public sealed class UsersDatabase : DbContext
    {

        // Stl.Fusion.EntityFramework tables
        public DbSet<UserEntity> Users { get; protected set; } = null!;
        public DbSet<UserIdentityEntity> UserIdentities { get; protected set; } = null!;
        public DbSet<SessionInfoEntity> Sessions { get; protected set; } = null!;
        public DbSet<DbKeyValue> KeyValues { get; protected set; } = null!;
        public DbSet<DbOperation> Operations { get; protected set; } = null!;

        public UsersDatabase(DbContextOptions<UsersDatabase> options)
            : base(options)
        {
            
        }
    }
}