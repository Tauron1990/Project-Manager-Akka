using System.Collections.Immutable;
using SimpleProjectManager.Server.Data;
using SimpleProjectManager.Server.Data.LiteDbDriver;
using SimpleProjectManager.Server.Data.MongoDb;
using Tauron.AkkaHost;

namespace SimpleProjectManager.Server.Configuration.ConfigurationExtensions;

public sealed class ProjectionStartConfig : ConfigExtension
{
    private readonly ImmutableDictionary<string, IDatabaseModule> _databaseModules = ImmutableDictionary<string, IDatabaseModule>.Empty
       .Add("mongodb", new MongoDbDatabaseDriver())
       .Add("litedb", new LiteDbDriverModule());

    public override void Apply(ImmutableDictionary<string, string> propertys, IActorApplicationBuilder applicationBuilder)
    {
        var module = _databaseModules[propertys["databaseDriver"]];
        
        module.Configurate(applicationBuilder, propertys);
    }
}