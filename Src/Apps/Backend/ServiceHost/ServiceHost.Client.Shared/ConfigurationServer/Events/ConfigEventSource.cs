using Akka;
using Akka.Streams.Dsl;

namespace ServiceHost.ClientApp.Shared.ConfigurationServer.Events;

public sealed record ConfigEventSource(Source<IConfigEvent, NotUsed> Source);