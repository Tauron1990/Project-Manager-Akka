using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Configuration.Hocon;
using Tauron.Application.AkkaNode.Services.Core;
using Tauron.Features;

namespace AkkaTest
{
    internal static class Program
    {
        private static async Task Main()
        {
            const string akka = "akka.conf";
            string config = await File.ReadAllTextAsync(akka);
            config = Extract(ConfigurationFactory.ParseString(config).Root.GetObject()).AggregateString((builder, s) => builder.AppendLine(s));

            Console.ReadKey();
        }

        private static List<string> Extract(HoconObject config)
        {
            List<string> elements = new();

            foreach (var (key, hoconValue) in config.Items.Where(hv => !hv.Value.IsEmpty))
            {
                var ele = key;
                    if (hoconValue.IsString())
                        elements.Add(ele + "=" + hoconValue.GetString());
                    else if (hoconValue.IsArray())
                        elements.Add(ele + "=" + hoconValue.GetArray().AggregateString((builder, value) => builder.Length == 0
                                                                                           ? builder.Append("[ ").Append(value.GetString())
                                                                                           : builder.Append(", ").Append(value.GetString())) + " ]");
                    else if(hoconValue.IsObject())
                        elements.AddRange(Extract(hoconValue.GetObject()).Select(s => ele + "." + s));
                    else
                        elements.Add(ele + "=" + hoconValue);

            }

            return elements;
        }

        private static string AggregateString<TType>(this IEnumerable<TType> enumerable, Func<StringBuilder, TType, StringBuilder> aggregator) 
            => enumerable.Aggregate(new StringBuilder(), aggregator).ToString();
    }
}