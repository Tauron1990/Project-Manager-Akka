using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using SimpleProjectManager.Operation.Client.Device.MGI.UVLamp.Events;

namespace SimpleProjectManager.Operation.Client.Device.MGI.UVLamp
{
    public sealed class DataFetcher : IDisposable
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

        private readonly HttpClient _webClient = new()
                                                 {
                                                     Timeout = TimeSpan.FromSeconds(2),
                                                 };
        private readonly MgiOptions _mgiOptions;


        public DataFetcher(MgiOptions mgiOptions)
            => _mgiOptions = mgiOptions ;

        public void Dispose() => _webClient.Dispose();

        public async Task<TrackingEvent> Handle(TickEvent msg)
        {
            try
            {
                string trackingString = await _webClient.GetStringAsync($"http://{_mgiOptions.Ip}/html/top.html?SysStatusData?")
                   .ConfigureAwait(false);

                string[] elements = trackingString.Split("&");

                var pairs = elements.Select(s => s.Split("=")).Select(ele => new ValuePair { Name = ele[0], Value = ele[1] }).ToArray();

                int power = int.Parse(
                    pairs.First(p => string.Equals(p.Name.ToLower(CultureInfo.InvariantCulture), "power", StringComparison.Ordinal))
                       .Value,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture);

                State state = StatesMapping
                [
                    pairs.First(p => string.Equals(p.Name.ToLower(CultureInfo.InvariantCulture), "sysstate", StringComparison.Ordinal))
                       .Value.ToLower(CultureInfo.InvariantCulture)
                ];

                double pidout = double.Parse(
                    pairs.First(
                            p => string.Equals(
                                p.Name.ToLower(CultureInfo.InvariantCulture),
                                "pidout",
                                StringComparison.Ordinal))
                       .Value,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture) / 10;

                int pidSetValue = int.Parse(
                    pairs.First(
                            p => string.Equals(
                                p.Name.ToLower(CultureInfo.InvariantCulture),
                                "pidsetvalue",
                                StringComparison.Ordinal))
                       .Value,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture);

                int pt1000 = int.Parse(
                    pairs.First(
                            p => string.Equals(
                                p.Name.ToLower(CultureInfo.InvariantCulture),
                                "pt1000",
                                StringComparison.Ordinal))
                       .Value,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture);

                return new TrackingEvent(power, state, pidout, pidSetValue, pt1000);
            }
            catch (Exception e)
            {
                #pragma warning disable EPC12
                return new TrackingEvent(Error: true, e.Message);
                #pragma warning restore EPC12
            }
        }

        private readonly record struct ValuePair(string Name, string Value)
        {
            public override string ToString() => $"{Name}={Value}";
        }
    }
}