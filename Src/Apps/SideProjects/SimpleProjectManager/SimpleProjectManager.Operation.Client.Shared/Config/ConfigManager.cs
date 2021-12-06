using Newtonsoft.Json;
using Tauron;
using Tauron.Application;
using Tauron.Application.VirtualFiles;

namespace SimpleProjectManager.Operation.Client.Shared.Config;

public sealed class ConfigManager
{
    private readonly IFile _location;
    
    public OperationConfiguration Configuration { get; private set; }
    
    public ConfigManager()
    {
        _location = new TauronEnviromentImpl(new VirtualFileFactory()).LocalApplicationData.GetDirectory("SimpleProjectManager").GetFile("ClientOperations");

        if (_location.Exist)
        {
            using var stream = _location.OpenRead();
            Configuration = JsonConvert.DeserializeObject<OperationConfiguration>(stream.ReadToEnd()) ?? new OperationConfiguration();
        }
        else
            Configuration = new OperationConfiguration();
    }

    public async ValueTask Set(OperationConfiguration configuration)
    {
        Configuration = configuration;
        await using var stream = new StreamWriter(_location.CreateNew());
        await stream.WriteAsync(JsonConvert.SerializeObject(Configuration));
    }
}