using Akka;
using Akka.Streams.Dsl;

namespace ServiceHost.Client.Shared.ConfigurationServer.Events;

public sealed record ConfigEventSource(Source<IConfigEvent, NotUsed> Source);