﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Configuration;
using Akka.Configuration.Hocon;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ServiceManager.Server.Controllers;
using ServiceManager.Shared.Api;

namespace ServiceManager.Server.Hubs
{
    public sealed class ClusterInfoHub : Hub
    {
        private readonly ILogger<ClusterInfoHub> _log;

        public ClusterInfoHub(ILogger<ClusterInfoHub> log)
            => _log = log;

        public Task SentPropertyChanged(string type, string name)
            => Clients.All.SendAsync(HubEvents.PropertyChanged, type, name);

        [HubMethodName("GetConfigFileOptions")]
        [UsedImplicitly]
        [Authorize]
        public ConfigOptionList GetBaseConfigOptions(ConfigOpensElement name)
        {
            try
            {
                var value = ConfigurationController.GetConfigData(name);

                if (string.IsNullOrWhiteSpace(value)) return new ConfigOptionList(Error: true, "Option nicht gefunden", Array.Empty<ConfigOption>());

                var hoconObject = ConfigurationFactory.ParseString(value).Root.GetObject();

                return new ConfigOptionList(Error: false, string.Empty, Extract(hoconObject).Select(s => new ConfigOption(s.Path, s.DefaultValue)).ToArray());
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error on Get Base Config Options");

                return new ConfigOptionList(Error: true, e.Message, Array.Empty<ConfigOption>());
            }


            static IEnumerable<(string Path, string DefaultValue)> Extract(HoconObject config)
            {
                foreach (var (key, hoconValue) in config.Items.Where(hv => !hv.Value.IsEmpty))
                {
                    var ele = key;

                    if (hoconValue.IsString())
                        yield return (ele, hoconValue.GetString() ?? string.Empty);

                    else if (hoconValue.IsArray())
                        yield return (ele, AggregateString(
                                          hoconValue.GetArray(),
                                          (builder, value) => builder.Length == 0
                                              ? builder.Append("[ ").Append(value.GetString())
                                              : builder.Append(", ").Append(value.GetString())) + " ]");

                    else if (hoconValue.IsObject())
                        foreach (var result in Extract(hoconValue.GetObject()).Select(s => (ele + "." + s.Path, s.DefaultValue)))
                            yield return result;

                    else
                        yield return (ele, hoconValue.ToString());
                }
            }

            static string AggregateString<TType>(IEnumerable<TType> enumerable, Func<StringBuilder, TType, StringBuilder> aggregator)
                => enumerable.Aggregate(new StringBuilder(), aggregator).ToString();
        }
    }
}