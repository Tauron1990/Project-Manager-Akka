using JetBrains.Annotations;

namespace Tauron.Application.Workshop;

[PublicAPI]
public interface IWorkDistributor<in TInput>
{
    void PushWork(TInput workLoad);
}