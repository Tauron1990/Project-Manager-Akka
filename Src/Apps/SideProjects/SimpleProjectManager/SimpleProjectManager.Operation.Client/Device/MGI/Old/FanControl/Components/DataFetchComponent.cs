using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Core.FanControl.Bus;
using SimpleProjectManager.Operation.Client.Device.MGI.UVLamp.Events;

namespace Akka.MGIHelper.Core.FanControl.Components
{
    public sealed class DataFetchComponent : IHandler<TickEvent>, IDisposable
    {
        private static readonly Dictionary<string, State> StatesMapping = new()
                                                                          {
                                                                              { "Idle".ToLower(), State.Idle },
                                                                              { "Ready".ToLower(), State.Ready },
                                                                              { "Ignition".ToLower(), State.Ignition },
                                                                              { "Start_up".ToLower(), State.StartUp },
                                                                              { "Stand_by".ToLower(), State.StandBy },
                                                                              { "Power".ToLower(), State.Power },
                                                                              { "Cool_down".ToLower(), State.Cooldown },
                                                                              { "Test_run".ToLower(), State.TestRun },
                                                                              { "Error".ToLower(), State.Error }
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
                var trackingString = await _webClient.GetStringAsync($"http://{_options.Ip}/html/top.html?SysStatusData?");

                var elements = trackingString.Split("&");

                var pairs = elements.Select(s => s.Split("=")).Select(ele => new ValuePair { Name = ele[0], Value = ele[1] }).ToArray();

                var power = int.Parse(pairs.First(p => p.Name.ToLower() == "power").Value);
                var state = StatesMapping[pairs.First(p => p.Name.ToLower() == "sysstate").Value.ToLower()];
                var pidout = double.Parse(pairs.First(p => p.Name.ToLower() == "pidout").Value) / 10;
                var pidSetValue = int.Parse(pairs.First(p => p.Name.ToLower() == "pidsetvalue").Value);
                var pt1000 = int.Parse(pairs.First(p => p.Name.ToLower() == "pt1000").Value);

                await messageBus.Publish(new TrackingEvent(power, state, pidout, pidSetValue, pt1000));
            }
            catch (Exception e)
            {
                #pragma warning disable EPC12
                await messageBus.Publish(new TrackingEvent(Error: true, e.Message));
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