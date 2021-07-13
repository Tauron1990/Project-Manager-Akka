using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Configuration;
using Akka.Configuration.Hocon;
using Microsoft.AspNetCore.Mvc;
using ServiceManager.Server.Properties;
using ServiceManager.Shared.Api;

namespace ServiceManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        [HttpGet]
        [Route(nameof(ConfigurationRestApi.GetBaseConfig))]
        public ActionResult<StringApiContent> GetBaseConfig(string name)
        {
            var value = GetConfigData(name);

            if (string.IsNullOrWhiteSpace(value)) return NotFound();

            return new StringApiContent(value);
        }

        [HttpGet]
        [Route(nameof(ConfigurationRestApi.GetBaseConfigOptions))]
        public ActionResult<ConfigOptionList> GetBaseConfigOptions(string name)
        {
            try
            {
                var value = GetConfigData(name);

                if (string.IsNullOrWhiteSpace(value)) return NotFound();

                var hoconObject = ConfigurationFactory.ParseString(value).Root.GetObject();

                return new ConfigOptionList(Extract(hoconObject).Select(s => new ConfigOption(s.Path, s.DefaultValue)).ToArray());
            }
            catch (Exception e)
            {
                return Problem(e.Message);
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
                    {
                        foreach (var result in Extract(hoconValue.GetObject()).Select(s => (ele + "." + s.Path, s.DefaultValue)))
                            yield return result;
                    }

                    else
                        yield return (ele, hoconValue.ToString());

                }
            }

            static string AggregateString<TType>(IEnumerable<TType> enumerable, Func<StringBuilder, TType, StringBuilder> aggregator)
                => enumerable.Aggregate(new StringBuilder(), aggregator).ToString();
        }

        private static string? GetConfigData(string name) => string.IsNullOrWhiteSpace(name) ? null : Resources.ResourceManager.GetString(name);
    }
}
