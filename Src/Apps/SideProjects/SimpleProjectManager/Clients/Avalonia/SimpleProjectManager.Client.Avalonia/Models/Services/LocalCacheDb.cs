using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Tauron.Applicarion.Redux.Extensions.Cache;

namespace SimpleProjectManager.Client.Avalonia.Models.Services;

public sealed class LocalCacheDbContextDesingFactory : IDesignTimeDbContextFactory<LocalCacheDbContext>
{
    public LocalCacheDbContext CreateDbContext(string[] args) 
        => new();
}

public sealed class LocalCacheDbContext : DbContext
{
    public DbSet<CacheData> Data => Set<CacheData>();

    public DbSet<CacheTimeout> Timeout => Set<CacheTimeout>();


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionStringBuilder = new SqliteConnectionStringBuilder
                                      {
                                          DataSource = Path.Join(LocalCacheDb.DatabaseDirectory, "Cache.db")
                                      };

        optionsBuilder.UseSqlite(connectionStringBuilder.ConnectionString);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CacheData>()
           .Property(cd => cd.Id)
           .HasConversion(id => id.Value, s => new CacheDataId(s));

        modelBuilder.Entity<CacheTimeout>()
           .Property(ct => ct.Id)
           .HasConversion(id => id.Value, s => new CacheTimeoutId(s));

        modelBuilder.Entity<CacheTimeout>()
           .Property(ct => ct.DataKey)
           .HasConversion(dk => dk.Value, s => new CacheDataId(s));
        
        base.OnModelCreating(modelBuilder);
    }
}

public sealed class LocalCacheDb : ICacheDb
{
    public static readonly string DatabaseDirectory = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SimpleProjectManager");

    private readonly Func<LocalCacheDbContext> _contextFactory;

    public LocalCacheDb(IServiceProvider serviceProvider)
    {
        if (!Directory.Exists(DatabaseDirectory))
            Directory.CreateDirectory(DatabaseDirectory);

        _contextFactory = serviceProvider.GetRequiredService<LocalCacheDbContext>;

        using var factory = _contextFactory();
        factory.Database.Migrate();
    }

    public async ValueTask DeleteElement(CacheTimeoutId key)
    {
        await using var context = _contextFactory();

        var data = await context.Timeout.FindAsync(key);
        if(data == null) return;

        context.Timeout.Remove(data);
        await context.SaveChangesAsync();
    }

    public async ValueTask DeleteElement(CacheDataId key)
    {
        await using var context = _contextFactory();

        var data = await context.Data.FindAsync(key);
        if (data != null)
        {
            context.Remove(data);
            await context.SaveChangesAsync();
        }
        
        await DeleteElement(CacheTimeoutId.FromCacheId(key));
    }

    public async ValueTask<(CacheTimeoutId? id, CacheDataId? Key, DateTime Time)> GetNextTimeout()
    {
        await using var context = _contextFactory();

        var result = await 
        (
            from data in context.Timeout.AsNoTracking()
            orderby data.Timeout
            select data
        ).FirstOrDefaultAsync();

        return result == null ? default : (result.Id, result.DataKey, result.Timeout);
    }

    public async ValueTask TryAddOrUpdateElement(CacheDataId key, string dataString)
    {
        await using var context = _contextFactory();

        var timeoutKey = CacheTimeoutId.FromCacheId(key);
        
        var timeoutData = await context.Timeout.FindAsync(timeoutKey);
        var data = await context.Data.FindAsync(key);

        if (timeoutData == null)
            context.Timeout.Add(new CacheTimeout(timeoutKey, key, GetTimeout()));
        else
            context.Timeout.Update(timeoutData with { DataKey = key, Timeout = GetTimeout() });

        if (data == null)
            context.Data.Add(new CacheData(key, dataString));
        else
            context.Data.Update(data with { Data = dataString });

        await context.SaveChangesAsync();
    }

    public async ValueTask<string?> ReNewAndGet(CacheDataId key)
    {
        await using var context = _contextFactory();

        var timeoutKey = CacheTimeoutId.FromCacheId(key);

        var timeoutData = await context.Timeout.FindAsync(timeoutKey);
        var data = await context.Data.FindAsync(key);

        if (data == null) return string.Empty;

        if (timeoutData == null)
            context.Timeout.Add(new CacheTimeout(timeoutKey, key, GetTimeout()));
        else
            context.Timeout.Update(timeoutData with { Timeout = GetTimeout() });

        await context.SaveChangesAsync();

        return data.Data;
    }
    private static DateTime GetTimeout()
        => DateTime.UtcNow + TimeSpan.FromDays(7);
}