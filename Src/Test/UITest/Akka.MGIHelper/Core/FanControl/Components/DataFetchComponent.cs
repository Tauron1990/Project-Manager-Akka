using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Core.FanControl.Bus;
using Akka.MGIHelper.Core.FanControl.Events;

namespace Akka.MGIHelper.Core.FanControl.Components
{
    public sealed class DataFetchComponent : IHandler<TickEvent>, IDisposable
    {
        private static readonly Dictionary<string, State> StatesMapping = new(StringComparer.Ordinal)
                                                                          {
                                                                              { "idle", State.Idle },
                                                                              { "ready", State.Ready },
                                                                              { "ignition", State.Ignition },
                                                                              { "start_up", State.StartUp },
                                                                              { "stand_by", State.StandBy },
                                                                              { "power", State.Power },
                                                                              { "cool_down", State.Cooldown },
                                                                              { "test_run", State.TestRun },
                                                                              { "error", State.Error },
                                                                          };

        private readonly FanControlOptions _options;

        private readonly HttpClient _webClient = new()
                                                 {
                                                     Timeout = TimeSpan.FromSeconds(2)
                                                 };

        public DataFetchComponent(FanControlOptions options) => _options = options;

        public void Dispose() => _webClient.Dispose();

        public async Task Handle(TickEvent msg, MessageBus messageBus)
        {
            try
            {
                string trackingString = await _webClient.GetStringAsync($"http://{_options.Ip}/html/top.html?SysStatusData?").ConfigureAwait(false);

                string[] elements = trackingString.Split("&");

                ValuePair[] pairs = elements.Select(s => s.Split("=")).Select(ele => new ValuePair { Name = ele[0], Value = ele[1] }).ToArray();

                int power = int.Parse(
                    pairs.First(p => string.Equals(p.Name.ToLower(CultureInfo.InvariantCulture), "power", StringComparison.Ordinal)).Value,
                    CultureInfo.InvariantCulture);
                State state = StatesMapping[pairs
                    .First(p => string.Equals(p.Name.ToLower(CultureInfo.InvariantCulture), "sysstate", StringComparison.Ordinal)).Value
                    .ToLower(CultureInfo.InvariantCulture)];
                double pidout = double.Parse(
                    pairs.First(p => string.Equals(p.Name.ToLower(CultureInfo.InvariantCulture), "pidout", StringComparison.Ordinal)).Value,
                    CultureInfo.InvariantCulture) / 10;
                int pidSetValue = int.Parse(
                    pairs.First(p => string.Equals(p.Name.ToLower(CultureInfo.InvariantCulture), "pidsetvalue", StringComparison.Ordinal)).Value,
                    CultureInfo.InvariantCulture);
                int pt1000 = int.Parse(
                    pairs.First(p => string.Equals(p.Name.ToLower(CultureInfo.InvariantCulture), "pt1000", StringComparison.Ordinal)).Value,
                    CultureInfo.InvariantCulture);

                await messageBus.Publish(new TrackingEvent(power, state, pidout, pidSetValue, pt1000)).ConfigureAwait(false);
            }
            catch (Exception e)
            {
#pragma warning disable EPC12
                await messageBus.Publish(new TrackingEvent(Error: true, e.Message)).ConfigureAwait(false);
#pragma warning restore EPC12
            }
        }

        private class ValuePair
        {
            internal string Name { get; init; } = string.Empty;

            internal string Value { get; init; } = string.Empty;

            public override string ToString() => $"{Name}={Value}";
        }
    }
}