using SimpleProjectManager.Operation.Client.Config;

namespace SimpleProjectManager.Operation.Client.Setup;

public interface ISetup
{
    ValueTask<OperationConfiguration> RunSetup(OperationConfiguration operationConfiguration);
}