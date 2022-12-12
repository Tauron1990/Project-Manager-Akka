using System.Diagnostics;
using Newtonsoft.Json;
using SimpleProjectManager.Operation.Client.Core;
using Tauron;
using Tauron.Application;
using Tauron.Application.VirtualFiles;

namespace SimpleProjectManager.Operation.Client.Config;

public sealed class ConfigManager
{
    private readonly IFile _location;

    public ConfigManager(IClientInteraction clientInteraction)
    {
        _location = new TauronEnviromentImpl(new VirtualFileFactory()).LocalApplicationData.GetDirectory("SimpleProjectManager").GetFile("ClientOperations");

        try
        {
            if(_location.Exist)
            {
                using TextReader stream = _location.OpenRead();
                Configuration = JsonConvert.DeserializeObject<OperationConfiguration>(stream.ReadToEnd()) ?? new OperationConfiguration();
            }
            else
            {
                Configuration = new OperationConfiguration();
            }
        }
        catch (Exception e)
        {
            if(clientInteraction.AskForCancel("Load Configuration", e))
                throw new OperationCanceledException("Cancel Setup", e);

            Configuration = new OperationConfiguration();
        }
    }

    public OperationConfiguration Configuration { get; private set; }

    [DebuggerStepThrough]
    public async ValueTask Set(OperationConfiguration configuration)
    {
        Configuration = configuration;
        var stream = new StreamWriter(_location.CreateNew());
        await using (stream.ConfigureAwait(false))
            await stream.WriteAsync(JsonConvert.SerializeObject(Configuration)).ConfigureAwait(false);
    }
}