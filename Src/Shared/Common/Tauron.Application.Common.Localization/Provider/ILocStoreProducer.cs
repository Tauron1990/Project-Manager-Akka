using Akka.Actor;

namespace Tauron.Application.Common.Localization.Provider;

public interface ILocStoreProducer
{
    string Name { get; }

    Props GetProps();
}